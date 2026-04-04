# Unity Project Structure

## Folders

- `Scripts/`
  - `Core/`
  - `Gameplay/`
  - `UI/`
  - `Services/`
  - `Installers/` or `Bootstrap/` for dependency composition

---

## Services

- `AuthService`
- `SupabaseService`
- `LevelService`
- `ScoreService`
- `TokenService`

All services should be injected into consumers rather than located globally at runtime.

---

## Gameplay

- `GridManager`
- `PlayerController`
- `LevelLoader`
- `MoveSystem`
- `GridBoard` for board bounds, blocked cells, and tile lookup only
- `GridGroundTile` for scene-authored playable floor cells
- `GridObstacle` for scene-authored blocked tiles, including vertical layer coordinates
- `GridKey`, `GridDoor`, and `PlayerKeyRing` for simple keyed-door puzzle interactions
- `GridMover` as the Unity-facing movement adapter for the player object
- `IPlayerAnimationController` and adapters for mapping movement intent to model-specific animator parameters

Gameplay rules should live in testable plain C# classes when possible, with `MonoBehaviour` classes acting as Unity-facing adapters.
Keep board-authoring data on a dedicated board object rather than scattering gameplay logic across floor pieces in the scene hierarchy.
When scenes are authored with placed floor and obstacle pieces, board bounds should be derived from those cells instead of stored as static width, height, or layer values.

---

## Scoring

- `MoveCounter`
- `ScoreCalculator`

---

## UI

- `MainMenu`
- `LevelSelect`
- `HUD`
- `EndLevelScreen`

---

## Implementation Rules

- Use dependency injection across gameplay, UI, and service layers.
- Avoid hidden singleton dependencies and static mutable state for core systems.
- When folder structure or responsibilities change, update this file in the same task.
- Prefer a dedicated board root for logic and keep floor visuals as separate handcrafted objects or prefabs aligned to the same grid.
