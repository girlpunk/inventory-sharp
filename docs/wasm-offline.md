# WASM / Offline-first Roadmap

Status: **planned** (decisions locked 2026-08-30).
Execute phase by phase; each phase is independently shippable and verifiable in the deployed test environment.

Context: the live app is the **server host** (InteractiveServer via `Routes`).
The WASM host in `BlazorInventory.Client` exists (`Microsoft.NET.Sdk.BlazorWebAssembly` project, `Program.cs`, `App.razor.cs` WASM branch) but is **not deployable** — no `wwwroot/index.html`, no WebSocket client registration, no serving.

See also `docs/requirements.md` (full product requirements).
This roadmap covers its §14 (Mobile & offline); Phases 1–2 also deliver the offline capability for §6 (container inventory sessions), §7 (foreign fetches with offline degradation), and are the long-term runtime for §12 (fixed readers).

## Decisions

| Question | Decision |
| --- | --- |
| Hosting | Sub-path on the **same deployment** (e.g. `/wasm`); keeps one deploy, same-origin cookies, trivial auth |
| Local store | **EF Core SQLite in WASM** (db file in IndexedDB/OPFS), reusing entity types + view models + Mapster |
| Offline writes (v1) | **Label scans + item/label create/update** (outbox); defer tags, photos, deletes, conflicts beyond last-write-wins |
| PWA | **After offline writes work** (Phase 3) |

## Phase 0 — WASM thin client (online)

Goal: a working WASM client that logs in and uses the existing services over the Fusion WebSocket; no offline capability yet.

1. **`BlazorInventory.Client/wwwroot/index.html`** — .NET 10 boot page (`blazor.web.js`).
   Verify the exact shape by scaffolding `dotnet new blazorwasm` (net10.0) at implementation time.
2. **Serve under a sub-path** in `BlazorInventory/BlazorInventory/Program.cs` — static files for the wasm build output + fallback to the wasm `index.html` (e.g. `/wasm`), alongside the existing `UsePathBase` for the server UI.
   Wasm app gets its own page with a matching base href.
3. **Uncomment the WASM wiring** in `ClientStartup.cs` (the `ConfigureServices` wasm branch only; the shared branch is already used by the server host):
   - `fusion.Rpc.AddWebSocketClient(...)` (same origin) — currently commented at `ClientStartup.cs:34-35` and `:71`; API: `ActualLab.Rpc.RpcBuilder.AddWebSocketClient` (`/mnt/d/Fusion/src/ActualLab.Rpc/RpcBuilder.cs:95-106`).
   - `fusion.AddAuthClient()` — commented at `ClientStartup.cs:29`; server side already has `fusionServer.AddAuthEndpoints()` wired.
4. **Auth**: same-origin sub-path should make cookie-based auth (Authentik OIDC via the server) work with minimal fuss — this is the first thing to validate.
5. **Verify**: wasm loads at `/wasm`, login works, items list over WebSocket; the already-registered `RpcPeerStateMonitor` (`ClientStartup.cs`, kept during the circuit-state work) now shows real browser↔server peer state in the wasm app.

Exit criteria: online WASM client sharing the exact same pages/services as the server UI.

## Phase 1 — Offline reads (local cache)

1. **`WasmDbContext`**: EF Core + SQLite, db file persisted in IndexedDB/OPFS; reuses existing entity types, view models, Mapster mappings; trimmed to what the UI reads (items, labels, scans, tags).
2. **Local compute services** in the wasm scope: `fusion.AddComputeService<TLocalItemService>(ServiceLifetime.Scoped)` etc. — the Fusion sample pattern (`/mnt/d/Fusion/samples/TodoApp/UI/ClientStartup.cs:112`).
   Pages inject `IItemService` → resolves to the local implementation in wasm, to the RPC client in the server host.
   **No page changes.**
3. **Sync service** (wasm hosted service): on connection (and on reconnect) full-pull from the server computes (`List`/`Get`) into SQLite.
   The UI always reads the local store, so offline browsing works automatically.

Note: `BlazorInventory/BlazorInventory/Data/Models/BaseModel.cs` has **no `Updated` timestamp** (only `Id`), so incremental sync is deferred — it would need an `Updated` column on the entities + migration + a `ListAfter` compute on the server.
Full-pull is fine for inventory-sized data.

Exit criteria: browse all data with no server connection.

## Phase 2 — Offline writes (outbox)

1. **Outbox table** in the local SQLite db: command payloads (generated ids, status, timestamps).
2. **Local write path**: online → execute via server RPC + update local db; offline → apply locally + enqueue (instant UI feedback).
   The scanner widget (`WebScan`) keeps working offline — scans queue up (the killer offline feature).
3. **Sync worker** (wasm hosted service): flushes the outbox on reconnect via the existing `UpdateCommand`s; idempotent by id, last-write-wins.
4. **v1 scope**: label scans, item/label create + update.
   Deferred: tags, photos (binaries), deletes, conflict resolution beyond LWW.

Exit criteria: disconnect, scan/add/edit, reconnect → changes land on the server exactly once.

## Phase 3 — PWA + polish

- PWA manifest + service worker (offline app shell) — .NET 10 wasm templates support this.
- Connection UX in the wasm app: `RpcPeerStateMonitor`-based indicator (registration already in `ClientStartup.cs`) + .NET 10's built-in reconnect modal.
  (The server-side `CircuitConnectionHandler` does not apply to wasm — there is no server circuit there.)
- **Test project** (none exists yet — see AGENTS.md): outbox/sync logic unit-tested without a browser; also the checkpoint for removing the `TEMP DIAGNOSTIC` log lines once the empty-lists-on-first-load bug is confirmed fixed in deployment.

## Verified references (checked 2026-08-30)

- Fusion **14.3.34** source at `/mnt/d/Fusion` (commit `61770c097c7b` = v14.3.34 release):
  - `src/ActualLab.Rpc/RpcBuilder.cs:95-106` — `AddWebSocketClient` overloads.
  - `samples/TodoApp/UI/ClientStartup.cs:103,112` — sample wasm wiring: `fusion.Rpc.AddWebSocketClient(remoteRpcHostUrl)` + `fusion.AddComputeService<Todos>(ServiceLifetime.Scoped)`.
  - `src/ActualLab.Fusion/Extensions/RpcPeerStateMonitor.cs` — peer-ref null = "always connected" (server); `RpcRef.Default` = real client peer (wasm).
- ASP.NET Core source (release/10.0) at `/mnt/d/aspnetcore` — circuit/reconnect internals if needed (see AGENTS.md "Reference" section).
- Repo: `BlazorInventory/BlazorInventory.Client` (WASM host project), `Client/ClientStartup.cs` (shared + wasm DI), `Client/App.razor.cs` (WASM branch), `BlazorInventory/BlazorInventory/Program.cs` (serving), `BlazorInventory/BlazorInventory/Data/Models/BaseModel.cs` (no `Updated`).

## Risks / unknowns

- Auth wiring in the wasm app is the first unknown to validate in Phase 0 (same-origin sub-path should keep cookies working).
- Bundle size: SQLite interop adds weight (AOT optional later).
- Consistency: no change feed / `Updated` column → full-pull until incremental sync is built.
