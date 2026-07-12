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
`SupabaseAuthService` is the current Unity-facing MVP auth adapter: it opens the
Supabase OAuth URL, receives the implicit-flow deep-link callback, extracts
Supabase access and refresh tokens from the callback URL fragment, stores those
tokens locally, and exposes authentication state to menu UI. Google OAuth requests
the `openid email profile` scopes through Supabase. On Android, the adapter also
checks the Activity intent data when the app starts or regains focus so auth
callbacks still work if Unity's deep-link event is missed.
- `LevelAttemptService` owns the local attempt/cooldown economy in plain C#.
  `LocalLevelAttemptGate` is the Unity-facing PlayerPrefs-backed adapter used by
  level start and restart UI until Supabase-backed validation is added.

---

## Gameplay

- `GridManager`
- `PlayerController`
- `LevelLoader`
- `MoveSystem`
- `GridBoard` for board bounds, blocked cells, inactive portal blocking, and tile lookup only
- `GridGroundTile` for scene-authored playable floor cells
- `GridObstacle` for scene-authored blocked tiles, including vertical layer coordinates
- `GridKey`, `GridDoor`, and `PlayerKeyRing` for simple keyed-door puzzle interactions
- `GridCollectible` for scene-authored objective pickups that gate portal activation
- `GridMover` as the Unity-facing movement adapter for the player object
- `LevelSceneMetadata`, `LevelExit`, and `LevelSceneFlowController` for level identity, completion, result saving, and next/menu navigation hooks
- `LevelCatalog` as the Unity-authored source of truth for official level order,
  stable level GUIDs, scene names, readable world/level labels, and star thresholds
- `LevelCatalogEntry` as one official level definition inside the catalog
- `LevelCompletionResult` as the plain C# result payload for completed levels
- `LevelObjectiveState` as the plain C# collectible progress and portal activation state
- `IPlayerAnimationController` and adapters for mapping movement intent to model-specific animator parameters and visual-facing rotation
- Player movement adapters may smooth visual travel and facing rotation as presentation, while grid validation, grid position updates, move counting, and interaction resolution stay tile/action based.

Gameplay rules should live in testable plain C# classes when possible, with `MonoBehaviour` classes acting as Unity-facing adapters.
Keep board-authoring data on a dedicated board object rather than scattering gameplay logic across floor pieces in the scene hierarchy.
When scenes are authored with placed floor and obstacle pieces, board bounds should be derived from those cells instead of stored as static width, height, or layer values.

---

## Scoring

- `MoveCounter`
- `ScoreCalculator`

During the MVP, star thresholds live in `LevelCatalog` and saved player score data
stores only best move counts by stable level GUID.

---

## UI

- `MainMenu`
- `LevelSelect`
- `HUD`
- `EndLevelScreen`
- `MainMenuAuthGate` for hiding the main menu and level select until either a
  Supabase session exists or the player confirms offline play after the warning
  prompt. Offline play is a UI access state only and must not be treated as an
  authenticated backend session.
- `GameplayHudController` for binding scene-authored HUD prefabs to `GridMover`, `PlayerKeyRing`, `LevelSceneFlowController`, and `LevelSceneMetadata`
- `LevelCompletionUiController` for binding scene-authored completion UI to moves, stars, retry, menu, and next-level actions

---

## Implementation Rules

- Use dependency injection across gameplay, UI, and service layers.
- Avoid hidden singleton dependencies and static mutable state for core systems.
- When folder structure or responsibilities change, update this file in the same task.
- Prefer a dedicated board root for logic and keep floor visuals as separate handcrafted objects or prefabs aligned to the same grid.
- Keep the main menu in its own scene and keep gameplay scenes free of title-screen UI composition.
