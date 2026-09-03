# Product Requirements — Inventory System

Status: **agreed** (2026-08-30).
This document captures the domain vision and functional requirements, with decisions locked where marked **[D]**.
It complements `docs/wasm-offline.md` (WASM/offline roadmap) and should be read alongside it.

## 1. Overview

A personal inventory system for tracking **physical objects** and their **physical locations** via a chain of containment (cupboard → shelf → bag → camera → SD card), with machine- and human-readable labels for identification, scan history as an audit/correction trail, and integration with other inventory systems.

**Primary users:** the owner (daily use) + a small number of other people with granted access.
**Client devices:** laptops (administration) + mobile devices (label interaction "on the go", poor-signal environments such as warehouses — see `docs/wasm-offline.md`).
**Non-goals (v1):** multi-site/multi-organization sharing, real-time collaboration, public web search over the inventory.

## 2. Domain model (current → target)

Existing (see `BlazorInventory/BlazorInventory/Data/Models/`): `Item` (single `ParentId`), `ItemLabel` (`Identifier` + `[Flags] LabelType` QR/UHF/RFID/GS1), `LabelScan` (label, scanner, geo, time), `Scanner` (name, type, geo, **`ParentItemId`** — fixed readers already associate with a container), `ItemPhoto`, `ItemTag` (flat string), `ForeignServer` (namespace + URI).

Target changes:

| Area | Change |
| --- | --- |
| `Item` | Add `Updated` timestamp; `Missing` flag + `MissingSince` (+ context of where it was expected); short `UrlCode` for public QR URLs. Keep **single parent** **[D]**. |
| `ItemLabel` | Add `HumanReadableCode` (optional fallback when the machine code fails); for QR labels the `Identifier` holds the **full URL**; uniqueness per identifier. |
| `LabelScan` | Add device identity for web/mobile sessions (a browser session registers as a `Scanner` with a stable per-device id); link to `InventoryRun` (if part of one). |
| `ItemTag` | Restructure to **namespaced key + value (JSON or plain string)** **[D]** — e.g. `product:expiry-date`, `monitoring:log-id`. |
| `InventoryRun` (new) | A container-inventory session: container, expected-contents snapshot, per-item status (found / missing / extra / moved), start/end times, summary. |
| `ForeignServer` | Add auth (token) + adapter-type reference (see §7). |
| Root items | Convention, not code: special container items ("World", country, region, building, …). Nearly every item should have a parent; seeded once during setup. |

## 3. Items & location

- **[D] Single-parent tree.**
  One item = one physical location at a time.
  Duplicates are separate items (e.g. "spare SD card" as its own item).
- **Find-location:**
  the item details page shows the full containment path (breadcrumbs from item up to root, e.g. `World / House / Kitchen cupboard / Shelf 2 / Bag / Camera / SD card`) — clicking a segment navigates to that container.
- **Tree/browsing UI:** container items can be browsed hierarchically (folder-style navigation) in addition to the flat list.
- **Search:** the header search finds by name, description, human-readable label code, and tag.
- **Move = set parent:** Moving an item (e.g. bag from shelf 2 to a box) is an explicit edit, not implicit.

## 4. Labels

- **[D] Storage-method agnostic.**
  The system stores the identifier + type; it doesn't care how it's physically encoded.
  `LabelType` stays the vocabulary (QR, UHF, RFID, GS1); unknown/other data is stored as-is.
- **Human-readable fallback code** on every label (optional field); typed lookup (no scanner needed) must find the item by it.
- **QR codes carry usable URLs** **[D]**:
  printing a label generates a QR whose content is a public URL (`https://<host><base_path>/l/{urlcode}`).
  Scannable by any phone, no app installed — the lost-item flow (§9).
- **Label lookup (core operation):**
  `identifier → item`.
  A scan is "find the item that owns this label".
- **Multiple labels per item** is supported (already in the model); each label has a separate identifier.
- **Unknown label scan (local namespace):** prompt to create the item, or store the label as unassigned for later (UI decision at implementation; both allowed).

## 5. Scans

- Every label read creates a `LabelScan`: label, **device** (registered scanner id; web/mobile sessions auto-register a `Scanner` with a per-browser persistent id), **method/type**, timestamp, **geolocation** (optional).
- **Geolocation default:** prompt per scan on mobile (default off — user opts in); off on laptop; fixed scanners carry static coordinates on the device.
- **Purpose:**
  a location audit trail.
  Item history view shows past scans (when, where, by what device).
- A plain scan **does not change** the item's parent.
  Location correction happens via: explicit edit, inventory session (§6), fixed-reader association (`Scanner.ParentItemId`), or the missing-item flow (§6.4).

## 6. Container inventory ("inventory a container")

Workflow: start an `InventoryRun` for a container item → expected contents = its **direct children only** **[D]** (nested containers are themselves expected items, but *their* contents are not checked) → user scans everything physically inside → **live report** as progress is made.

- **Report while in progress:**
  three lists — found / not yet found / unexpected; plus a session summary at the end.
  The run is persisted (resumable across sessions for large containers).
- **Scan report:** every scan during the run is a normal `LabelScan` linked to the run — "inventorying should still generate scan reports".
- **Unexpected item:** option to move it into the container (set parent), move it elsewhere (picker), or just record it.
- **Missing items:** mark one or more as missing → sets `Item.Missing` + `MissingSince` (+ the container it was expected in).
- **Missing-item alert** **[D]:** scanning a missing item shows an alert ("expected in {container} since {date}") and **prompts to set the item's new parent** from the current scan context (inventory container / scanner's `ParentItemId`); confirming clears the flag.
- Run summary available in-app; export as JSON/CSV.

## 7. Foreign systems (other inventories)

- **Adapter interface, no concrete first adapter yet** **[D]** — the external service "L" API format is unknown (info requested).
  Define a pluggable interface (e.g. `IForeignInventoryAdapter`) keyed per `ForeignServer` (namespace + URI + auth):
  - `Resolve(identifier) → ForeignItemInfo?` (name, description, photos, extra fields) — called when a scanned label belongs to a foreign namespace.
  - `PushScan(scan) → Task` — optional: forward the scan record to the other system.
- **[D] Materialize locally:**
  on first foreign scan, fetch details and create a full local item (marked as foreign, "last fetched" stamped).
  Works offline afterwards; re-fetchable to refresh.
- Unknown foreign labels where the remote is unreachable: store as an unassigned/pending label (retry on reconnect — aligns with the offline client in `docs/wasm-offline.md`).
- Concrete adapters (external service L first, once the API is documented) are follow-up work; the interface + config + materialization flow are v1.

## 8. Product code decoding (factory-printed labels)

Many products (food, etc.) ship with QR/barcodes encoding product information (GS1 for medication already in `LabelType`).

- **Decoder interface** **[D]:** a pluggable `IProductCodeDecoder` (registered by code prefix/format): `code → DecodedProductInfo?` (name, brand, description, expiry date, lot, …).
- **v1 decoder:**
  GS1 (Application Identifiers — `(01)` GTIN, `(17)` expiry, `(11)` production date, `(10)` lot, …), which covers medication and many food products.
  Additional formats (other food QR schemes) are future adapters.
- **Flow:** scanning a code that decodes to a product (and has no local label) → prompt to create the item with decoded data pre-filled (name/description); expiry & lot become **tags** (`product:expiry-date`, …) — which is what feeds the food-expiry plugin later (§9).

## 9. Tags & plugins

- **[D] Tag = namespaced key + value (JSON or plain string).** Namespace = plugin/feature scope (e.g. `product:*`, `monitoring:*`).
- **Plugin system (foundation in v1):** a plugin can
  1. declare a tag namespace (read/write its own keys),
  2. contribute rendered info to the item view,
  3. provide a **report** (list of items matched by its tags),
  4. react to scan/inventory events (raise alerts in the UI),
  5. emit **notifications**.
- **v1 plugins:**
  *External reference display* — tags referencing other systems (log-aggregation/monitoring IDs for laptops, etc.) are displayed on the item page and exposed via the API.
  No logic beyond reading.
- **Next plugin (first real one):** food expiry — report of items expiring soon + alert during inventory that a food should be discarded (data comes from §8 decoding).
- **Later:**
  laundry — batch RFID load → wash program + problematic items; depends on the fixed reader hardware (§12) and the offline client.
  Washing-tag example: `laundry:program` (JSON) + `laundry:flags` (e.g. "no tumble dry").
- **Notifications, v1:**
  in-app list/alerts only.
  Email (via Authentik/SMTP) is a later extension point.

## 10. Public QR / lost-item flow

- **[D] Auth-aware single URL:** one URL per item.
  Logged-in users → normal item page.
  Anonymous → minimal "found it" card: **item name + a *global* "if found" text** (a single configured message, not per-item).
  No location, no inventory, no emails of any person.
- **Future:** "please leave your details" form on the public card (anonymous contact capture → owner follows up).
- The public page must work without authentication/cookies and must not leak anything beyond name + global message (+ possibly a generic item photo — decision left open, default: no photos for v1).

## 11. REST API (for other apps with Authentik tokens)

- **[D] REST** (ASP.NET Core controllers over the existing Fusion services), accepting Authentic-issued OIDC tokens.
  Usable from any language.
- **v1 scope — generic inventory API** **[D]**: items (CRUD, search, location path), labels (create, lookup by identifier), scans (create, history), tags (read/write), photos (upload/list).
- **First consumers, in order** **[D]:**
  1. Home dashboard — reads item locations (read-only first),
  2. Monitoring/log system — writes tags,
  3. Laundry reader app — batch scans (batch-scan endpoint can wait for the hardware, §12).
- **Auth:** validate Authentik OIDC tokens (user tokens for 1 & 2; client-credentials considered for service consumers).
  v1 = single authenticated-user role; role split (admin vs user) is a later refinement, expected to map from Authentik groups.
- The Blazor UI keeps using Fusion RPC internally; the REST API is a separate facade over the same services.

## 12. Fixed readers (always-on scanners)

- Physical fixed scanners (RFID reader at a washing machine, door readers, …) are **built later, not in v1** **[D]**.
- The seam already exists: `Scanner.ParentItemId` (a fixed reader's scans are associated with a container) — keep and preserve it.
- v2: batch-scan endpoint (a laundry load = many tags in one burst), plugin consumption of batch results, and the laundry plugin proper.

## 13. Auth & access

- **[D] Auth entirely via Authentik** — no passwords or 2FA secrets in this application (OIDC already wired; `App` scheme + Fusion session).
- Other people: access = Authentik group membership → app roles (v1: one role for all authenticated users).
- "Other apps" get tokens from Authentik (client credentials / authorization code) → REST API (§11).
- Public (unauthenticated) access is only the found-it card (§10).

## 14. Mobile & offline

Covered by `docs/wasm-offline.md` (phases, decisions).
Cross-references created by this document:

- The mobile client **is** the offline WASM/PWA app (phases 0–3 there).
- §6 inventory sessions must work offline (local store + outbox — their Phase 1–2).
- §7 foreign fetches must degrade gracefully offline (pending labels, retry on reconnect).
- §12 fixed readers are the long-term consumer of the offline client + batch API.
- Device identity for scans (§5) = the persistent per-browser id in the PWA.

## 15. Foundations (must-fix before new features build on top)

From AGENTS.md "Known issues":

1. **ID bug:** no GUID generation on create — `Id` stays `Guid.Empty`.
   Fix in `CRUDService.Update` create branch (application-level `Guid.NewGuid()`; also covers future WASM local writes) or a DB default in a migration.
2. **Empty-lists-on-first-load:** fixes deployed (`1291ec9`) — verify in the deployed environment, then remove the `TEMP DIAGNOSTIC` lines (`CRUDService`, `LabelService`, `Routes.razor.cs`, `Program.cs`).
3. **`Updated` timestamp** on all entity models (enables incremental sync later; see `docs/wasm-offline.md` Phase 1 note) + migration.
4. **Test project** (none exists): start with model/service unit tests; outbox/sync logic tests come with the offline work.
5. **UI cleanup:** demo menu items ("Submenu 1", "Item 3") and the placeholder search in `MainLayout` need real content (real nav + search per §3).

## 16. Suggested work order

1. **Foundations (§15)** — unblocks everything.
2. **Core domain:** tree browsing + find-location breadcrumbs + search; label `HumanReadableCode` + QR URL generation + public found-it card; browser-session device tracking + geolocation flow.
3. **Container inventory (§6)** — `InventoryRun` + missing alerts + reports.
4. **REST API v1 (§11)** — first consumer (home dashboard) can start integrating.
5. **Foreign adapters (§7) + product decoding (§8)** — interface + config + materialization + GS1 decoder; external service "L" adapter when its API docs arrive.
6. **Plugin system + food-expiry plugin (§9)** (tags restructure + plugin foundation land with step 2/3 so tags exist when decoding arrives).
7. **WASM/offline phases (docs/wasm-offline.md)** — Phase 0 (online WASM host) can also start any time after foundations; Phases 1–3 follow per that roadmap.
8. **Fixed readers + laundry (§12)** — hardware-gated, after the offline client.

## 17. Open items

| Item | Status |
| --- | --- |
| external service "L" API format (endpoints, auth, payload) | Waiting on info from the other party; blocks only the concrete adapter, not the interface |
| Non-GS1 product code formats (specific food QR schemes) | To be collected as they're encountered; decoder interface absorbs them |
| Public card: include item photo? | Default no for v1; revisit |
| "Leave your details" form on public card | Future, not v1 |
| Role split (admin vs user) on API/UI | Later refinement; v1 = single authenticated role |
| Photo storage: byte[] in Postgres | Fine for v1; consider compression/object storage if sizes grow |
| Batch-scan endpoint | With fixed readers (§12), not v1 |
