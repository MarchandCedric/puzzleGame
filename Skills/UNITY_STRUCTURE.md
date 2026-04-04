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
- `GridBoard` for board bounds, blocked cells, and tile lookup
- `GridMover` as the Unity-facing movement adapter for the player object

Gameplay rules should live in testable plain C# classes when possible, with `MonoBehaviour` classes acting as Unity-facing adapters.
For early scene prototypes, keep board-authoring data on a dedicated board object rather than scattering floor pieces across the scene hierarchy.

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
- Prefer a dedicated board root with generated or grouped tile children so floor visuals stay aligned with logical grid coordinates.
