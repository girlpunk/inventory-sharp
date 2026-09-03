# AGENTS.md — inventory-sharp

Context for AI agents (and humans) working in this repo.

## Project

Blazor inventory management app: items, labels (RFID/QR via scanners), scans, tags, photos, foreign servers (sync with label hosts like "L").
.NET 10, single-server Fusion deployment (`HostKind.SingleServer`).

## Repo layout

```
inventory-sharp.sln
BlazorInventory.Abstractions/     # Shared: service interfaces, commands, view models (records, [MemoryPackable])
BlazorInventory/                  # Web host (server)
  BlazorInventory/                #   ASP.NET host: Program.cs, Pages (account), Components, Data (EF Core), Services (Fusion services)
  BlazorInventory.Client/         #   Blazor UI components (shared); WASM host files exist but are not deployable (no index.html, see Gotchas)
```

Key entry points:
- Server host: `BlazorInventory/BlazorInventory/Program.cs` (Fusion registration in `ConfigureFusionServices`).
- Server UI root: `BlazorInventory/BlazorInventory/Components/App.razor` → renders `Components/Routes.razor` with `@rendermode="PageRenderMode"` (`InteractiveServer` for normal pages; `null`/fully static SSR for `[ExcludeFromInteractiveRouting]` pages like `/Account/*` — the circuit's fresh `Router` can never resolve excluded routes, so they must not be interactive).
- WASM UI root: `BlazorInventory/BlazorInventory.Client/App.razor` (inherits `CircuitHubComponentBase`).
- Shared DI for both hosts: `BlazorInventory/BlazorInventory.Client/ClientStartup.cs` (`ConfigureSharedServices`).

## Stack

- .NET 10; ASP.NET Core Razor Components (InteractiveServer is the live mode; InteractiveWasm is registered but unused)
- ActualLab.**Fusion 14.3.34** (RPC + CQRS + computed state caching + entity operations)
- EF Core 10 + Npgsql (PostgreSQL), `AddDbContextServices<ApplicationDbContext>` with `AddNpgsqlOperationLogWatcher` (PG LISTEN/NOTIFY operation log)
- Mapster 10 + Mapster.EFCore (`ProjectToType<TViewModel>()` for all list/get queries)
- AntDesign Blazor (UI), MemoryPack (serialization), OpenTelemetry + Prometheus
- Auth: ASP.NET Core Identity (core only) + OIDC via Authentik (`Authentik` scheme in appsettings); Fusion session cookie `FusionAuth.SessionId` (28d, set by `app.UseFusionSession()`)

## Architecture: how data flows

- UI pages are `ComputedStateComponent<T>` (e.g. `Pages/Item/Index.razor`) that call `[ComputeMethod]` service methods (`ItemService.List` etc.) via injected RPC clients.
  Computes are **cached per session** in server memory; they only re-run after **invalidation** (triggered by a command) or auto-invalidation (default: never).
- Commands (`UpdateCommand<TView>`, `DeleteCommand<TView>`, `ScanLabelCommand`, ...) go through `Commander.Call` → service methods → `DbOperationScope` → EF write.
  On completion, Fusion runs an **invalidation pass**: the command handler is re-invoked with `Invalidation.IsActive == true`; that branch must call the compute methods whose results are stale (returns are discarded).
- Services derive from `CRUDService<TModel, TViewModel>` (`BlazorInventory.Services`) — generic `List`/`Count`/`Get`/`Update`/`Delete`; subclasses provide `DbSet`, `DoUpdate`.
  Non-CRUD services: `TagService`, `ScanService`, `ForeignServerService`.
- **Invalidation contract (important):** the write path stores affected ids in `context.Operation.Items` (`KeylessSet(guid)` for the row's own id, or `Set("key", value)` for related ids — see `TagService` `"itemId"`, `LabelService` `"id"`); the invalidation branch reads them back from **`context.Operation.Items`** (never `context.Items` — `CommandContext.Items` is a separate, never-populated bag).
- View models are `record`s with `Guid? Id`; DB models have `Guid Id` (`BaseModel`).

## Build / run

```bash
dotnet build BlazorInventory/BlazorInventory/BlazorInventory.csproj   # builds server + client + abstractions
```

No test project exists yet — add unit tests once the known bugs below are fixed.
The app cannot be run from this dev machine (needs PostgreSQL + Authentik OIDC); the user deploys the Docker image to a test environment and reports behavior.

Verification workflow: `dotnet build` locally, then **ask the user to commit and push** — the CI pipeline (MegaLinter + `docker build`) is the compile check for changes, and the user reports the pipeline state and the deployed app's behavior.

## CI / deploy

- `.github/workflows/docker-image.yml`: on push to `main` (and PRs) runs MegaLinter (oxsecurity, no `.mega-linter.yml` in repo → default rules; it can open auto-fix PRs), then builds and pushes a Docker image to GHCR (main pushes only).
- Dockerfile: `BlazorInventory/BlazorInventory/Dockerfile` (aspnet:10.0, `dotnet workload restore`, publishes with `UseAppHost=false`).
- `origin` is a fork (`github.com/girlpunk/inventory-sharp`); Renovate PRs merge through it.
- `appsettings.Development.json` contains the live dev DB password and OIDC client secret (committed deliberately, internal dev env).
  Don't commit new secrets; don't assume dev settings reach production.

## Known issues / current work

- **Bug (investigation ongoing, fix awaiting deployment verification): lists empty on first page load.** After a server restart, the first load of any Fusion-computed list (items, labels, scans, dashboard) shows "No items found" (a genuine empty compute result, no error); refresh or any write makes the data appear.
  Longstanding (predates the 2026-04 "fix item create form" commits).
  Suspects addressed in commit `1291ec9` (2026-08-25): (1) circuit-scoped Fusion session/`CircuitHub` was no longer initialized after `e0cabcf` replaced the server's `Client.App` with `Routes` — restored in `Components/Routes.razor.cs`; (2) `CRUDService.Update` never `KeylessSet` its row id (invalidation `KeylessGet` was broken); (3) `LabelService.Scan` invalidation read from the wrong items bag and its write path never set `"id"`; (4) a duplicate `AddOperationReprocessor()` registration existed (now a single one in `Program.cs`).
  **Temporary `DIAG ...` log lines (marked `TEMP DIAGNOSTIC`) are still in `CRUDService`, `LabelService`, `Routes.razor.cs`, and `Program.cs` — remove them once the bugs are confirmed fixed.**
- **Confirmed ID bug:** no code generates a GUID for new rows — there is no `Guid.NewGuid()` anywhere in the repo, `Items.id` (and other PKs) are plain `uuid` columns with no DB default, and the create path (`UpdateCommand` with `command.Obj.Id == null`) builds the entity via `typeof(TModel).CreateInstance()`, so `Id` stays `Guid.Empty`. Fix by generating ids in `CRUDService.Update`'s create branch (set `item.Id` before `SaveChangesAsync`) or adding a DB default in a migration.
  Verify against the live DB: `psql: select id from items;`.
- `BlazorInventory/BlazorInventory/Services/CRUDService.cs` contains a commented-out original `Create` method — reference for the pre-refactor invalidation behavior.
  `Abstractions/Command/CreateCommand.cs` is likewise disabled (`[MemoryPackable]` commented out); creation goes through `UpdateCommand` with `Id == null`.

## Gotchas

- `BlazorInventory.Client/wwwroot` has **no `index.html`** — the standalone WASM host is not deployable as-is; the live app is the server host with pages running InteractiveServer via `Routes`.
  `Client/App.razor.cs` has a WASM branch (`OSInfo.IsWebAssembly`: `Session.Default` + `HostedServices().Start()`) for a potential standalone host — don't remove or move it.
- `App.razor` (server) passes `SessionId`/`RenderModeKey` to `Routes` — `Routes` must keep those `[Parameter]` props and the `CircuitHubComponentBase` wiring, or the circuit loses its session (Blazor silently ignores unmatched params, so there's no visible error).
- `CircuitHub` is a scoped service; `Initialize(dispatcher, renderMode)` may be called exactly once per circuit instance (throws `AlreadyInitialized`).
  `RenderModeDef.GetOrDefault("")` returns the default (`"a"`/Auto).
- The `SessionResolver.Session` getter throws `NotInitialized` if no session is set on that DI scope; the request scope is set by `UseFusionSession()`, the circuit scope must be set explicitly (done in `Routes`/`Client.App`).
- Fusion's `DbHub.CreateDbContext` (computes) uses the app-registered pooled `IDbContextFactory`; `readWrite: false` only turns off change tracking, not results.
- `UseNpgsqlHintFormatter()` (Fusion EF) only appends `FOR UPDATE`-style hints when a query explicitly used the `DbHint` API — plain queries are unaffected.
- `Program.cs` runs `db.Database.MigrateAsync()` before `app.Run()`; migrations live in `BlazorInventory/BlazorInventory/Data/Migrations`.
- Deployment is behind a reverse proxy under a sub-path: `Program.cs` runs `app.UsePathBase(Configuration.GetValue("base_path"))` and `App.razor` sets `<base href>` from the same `base_path` key.

## Reference

- Fusion 14.3.34 source (exact commit `61770c097c7bbbcb5f9de72466f1102746d0ad82` = v14.3.34 release on `master`): checked out at `/mnt/d/Fusion` (re-fetch: `git clone https://github.com/ActualLab/Fusion && git checkout 61770c097c7b`).
  Useful files: `src/ActualLab.Fusion/Interception/ComputeMethod*.cs` (compute caching), `src/ActualLab.Fusion/Operations/Internal/InvalidatingCommandCompletionHandler.cs` (invalidation pass — `operation.Items` is passed to the invalidating pass), `src/ActualLab.CommandR/CommandContext.cs` (`Items` vs `Operation.Items`), `src/ActualLab.Fusion.EntityFramework/DbHub.cs`, `src/ActualLab.Fusion.Blazor/` (`CircuitHub`, `CircuitHubComponentBase`, `RenderModeDef`), `src/ActualLab.Fusion.Server/Middlewares/SessionMiddleware.cs` (session cookie).
- Project docs: `docs/requirements.md` (product requirements + locked decisions), `docs/wasm-offline.md` (WASM/offline roadmap).
- ASP.NET Core source (branch `release/10.0`): checked out at `/mnt/d/aspnetcore`.
  Blazor server-side circuit: `src/Components/Server/src/Circuits/CircuitHandler.cs` (public `CircuitHandler` — `OnCircuitOpenedAsync`/`OnConnectionUpAsync` (initial + every reconnect) / `OnConnectionDownAsync` / `OnCircuitClosedAsync`; resolved as **scoped `CircuitHandler`-derived services** from the per-circuit DI scope — `CircuitFactory.cs` `GetServices<CircuitHandler>()`), `CircuitHost.cs` (event dispatch, `Order`), `CircuitOptions.cs` (`DisconnectedCircuitRetentionPeriod` — circuit kept for client reattach).
  Client-side reconnect: `src/Components/Web.JS/src/Platform/Circuits/CircuitStartOptions.ts` (defaults: `maxRetries: 30`, backoff 0/5s/30s, `dialogId: 'components-reconnect-modal'`), `DefaultReconnectionHandler.ts` + `DefaultReconnectDisplay.ts` (built-in modal) / `UserSpecifiedDisplay.ts` (custom modal via `<div id="components-reconnect-modal">`, class toggles `components-reconnect-show/retrying/failed/...`, child ids `components-reconnect-current-attempt` / `components-seconds-to-next-attempt`, event `components-reconnect-state-changed` with states show/hide/retrying/failed/resume-failed/paused/rejected).
- User's DB is PostgreSQL (`Host=localhost;Database=app;Username=app` in `appsettings.Development.json`, overridden in deployment); OIDC provider is Authentik.
