# Unity Best Practices For PuzzleGame

## Scope

These practices apply to the Unity Android client for PuzzleGame, a deterministic grid-based puzzle game with a fixed isometric view.

## Project Structure

- Keep runtime code under feature-oriented folders while preserving the current high-level grouping: `Core`, `Gameplay`, `UI`, `Services`.
- Create a composition root for each scene or bootstrap flow to register dependencies explicitly.
- Keep editor-only tooling in separate `Editor/` folders.
- Isolate testable domain logic from Unity scene objects.

## Dependency Injection

- Use dependency injection for all non-trivial systems.
- Prefer plain C# services for rules, calculations, orchestration, and data access coordination.
- Use `MonoBehaviour` classes as thin adapters for Unity lifecycle, references, animation hooks, and view updates.
- Inject interfaces into consumers instead of creating dependencies inside components.
- Centralize object graph creation in installers or bootstrap classes instead of scattering setup logic.
- Do not use global service locators or mutable static singletons for gameplay systems.

## Gameplay Architecture

- Represent player position and interactions in logical grid coordinates first.
- Resolve one action completely before starting the next action.
- Keep movement validation, interaction resolution, and score updates deterministic.
- Treat animation as a presentation of already-approved gameplay state changes, not as the source of truth.
- Keep move counting in one authoritative system.
- Clear or lock input during transitions exactly as defined in `INPUT_BUFFER_AND_ACTION_FLOW.md`.

## Scene And Prefab Practices

- Keep scenes lightweight and focused on composition.
- Prefer reusable prefabs with explicit serialized references.
- Avoid hidden scene dependencies that break when a prefab is moved to another scene.
- Validate required references early and fail loudly in development builds.

## Data And Configuration

- Store level definitions in a versionable format that can round-trip with the planned level editor.
- Use ScriptableObjects only for static configuration, never as mutable runtime state containers shared across sessions.
- Keep score thresholds, token timers, and tuning values in explicit config assets or backend-driven data contracts.

## UI

- Separate view rendering from application logic.
- Keep HUD, menus, and end-level screens dependent on injected presenters, controllers, or services.
- Design for multiple Android aspect ratios and safe areas from the start.

## Performance

- Minimize per-frame polling for gameplay logic.
- Favor event-driven updates for UI and progression state.
- Keep allocations predictable during action resolution to avoid mobile GC spikes.
- Pool frequently reused effects or transient objects when profiling shows repeated instantiation cost.

## Testing

- Put pure gameplay rules in plain C# classes that can be covered with edit mode tests.
- Add tests for movement validation, scoring thresholds, token calculations, and unlock progression.
- Treat any deterministic rule that affects stars or puzzle solvability as test-worthy.

## Documentation Rule

- Any Unity code change that affects architecture, flow, folder structure, or gameplay behavior must update the corresponding markdown docs in the same task.
