# Runtime Service Lifecycle Guide

## Singleton Pattern Rules

### Rule 1: Never use `Destroy(gameObject)` in a singleton duplicate guard

If a singleton sits on a **shared host** (Camera, GameBootstrap, HUD, character), `Destroy(gameObject)` deletes the entire host — including all other components and children. The CameraShakeService bug (2026-06) was exactly this: a duplicate CameraShakeService on Main Camera called `Destroy(gameObject)`, deleting the Camera.

**Always use `Destroy(this)`** to remove only the duplicate component.

The **only** exception: a singleton that creates its own dedicated GameObject (e.g., `[AudioFeedbackService]`). Even then, `Destroy(this)` is preferred for consistency.

### Rule 2: Use `GetComponent<T>()` instead of `Instance` when spawning services

`CombatHitFeedbackHub.EnsureServices()` previously checked `CameraShakeService.Instance == null`. During Awake ordering, a serialized CameraShakeService may not have set `Instance` yet, causing `EnsureServices()` to add a duplicate. The duplicate's `Awake()` runs synchronously (setting `Instance`), then the original's `Awake()` detects a conflict and calls `Destroy`.

**Always check `GetComponent<T>()` on the target GO** before calling `AddComponent<T>()`.

### Rule 3: Do not serialize shared-host singletons in scene files

Services like `CameraShakeService` and `HitStopService` are added at runtime by `CombatHitFeedbackHub.EnsureServices()`. Serializing them in a scene creates the Awake-ordering race condition described in Rule 2.

SetupMenu must NOT call `AddComponent<CameraShakeService>()` on the Camera during scene generation.

## Current Service Inventory

| Service | Host Object | Pattern | Safety |
|---------|------------|----------|--------|
| `CameraShakeService` | Main Camera (shared) | `Destroy(this)` | Safe — shared host, component-only destroy |
| `HitStopService` | GameBootstrap (shared) | `Destroy(this)` | Safe — shared host, component-only destroy |
| `CombatHitFeedbackHub` | GameBootstrap (shared) | `Destroy(this)` | Safe — shared host, component-only destroy |
| `AudioFeedbackService` | Dedicated GO `[AudioFeedbackService]` | `Destroy(this)` | Safe — dedicated host |
| `WorldMarkerService` | Dedicated GO `[WorldMarkerService]` | `Destroy(this)` | Safe — dedicated host |
| `RuntimeCameraBootstrap` | GameBootstrap (shared) | No destroy guard | N/A — session flag prevents duplicate work |

## Static Services (No MonoBehaviour, No GO Risk)

| Service | Type | State Management |
|---------|------|-----------------|
| `SaveService` | `static class` | File I/O only |
| `SubFactionRegistry` | `static class` | `Initialize(database)` — never reset |
| `CharacterRuntimeRegistry` | `static class` | Auto-populated by `CharacterEntity.OnEnable/OnDisable` |

## Validator Checks

The VerticalSliceValidator (`LuoLuoTrip/Tools/Validation/Run Vertical Slice Validation`) checks:

1. **CameraShakeService on Main Camera** — warns if serialized (should be runtime-only)
2. **RuntimeCameraBootstrap in scene** — warns if missing
3. **Camera setup** — tag, component, enabled, targetTexture, cullingMask, CameraFollowController

## Adding New Singletons

When adding a new MonoBehaviour singleton:

1. **Decide the host**: Prefer a dedicated GO. If it must share, document why.
2. **Use `Destroy(this)`**: Never `Destroy(gameObject)` in the duplicate guard.
3. **Use `GetComponent<T>()`** when checking before `AddComponent<T>()`.
4. **Do not serialize** the service in scene files if it will also be added at runtime.
5. **Add a comment** on the class explaining the host and the `Destroy(this)` reason.
