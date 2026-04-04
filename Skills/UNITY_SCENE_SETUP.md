# Unity Scene Setup For PuzzleGame

## Purpose

Use this document as the default setup guide when creating a new Unity scene for `PuzzleGame`.

It defines the baseline scene properties that fit the project's current constraints:

- Android-first
- Fixed isometric presentation
- Orthographic camera
- Grid readability first
- UI that adapts cleanly to multiple phone aspect ratios

If a scene needs to break these rules, treat that as an intentional exception and document why.

## Target Screen Strategy

Design for mobile first, with portrait as the default planning layout unless the team explicitly decides otherwise.

Recommended UI reference resolution:

- `1080 x 1920`
- Match mode: `Scale With Screen Size`
- Match value: start at `0.5`, then tune only if testing shows a clear need

Rules:

- Do not stretch gameplay to fill every aspect ratio.
- Keep the puzzle board readable on the smallest target phones first.
- Use extra space on taller or wider devices for margin, breathing room, and non-critical UI.
- Respect safe areas for notches, rounded corners, and system overlays.

## Scene Defaults

For each gameplay scene, start with:

- One main orthographic camera
- One root gameplay container
- One UI canvas for HUD and menus
- One scene installer or bootstrap object for dependency wiring
- One event system if the scene contains Unity UI

Suggested root hierarchy:

- `SceneContext` or `Bootstrap`
- `Main Camera`
- `LevelRoot`
- `GameplayRoot`
- `BoardRoot`
- `UIRoot`
- `EventSystem`

Keep scenes focused on composition, not business logic.

## Main Camera

Use these defaults for gameplay scenes:

- Projection: `Orthographic`
- Clear Flags: `Solid Color`
- Rotation: fixed isometric angle, no runtime free rotation
- Position: driven by level framing needs, not by free camera controls
- Culling Mask: only the layers required for gameplay and scene visuals

Recommended isometric-style rotation starting point:

- X: `30`
- Y: `45`
- Z: `0`

Alternative common setup if visuals fit better:

- X: `35.264`
- Y: `45`
- Z: `0`

Rules:

- Do not allow player-controlled camera rotation.
- Avoid constant camera follow motion.
- Prefer stable framing that shows the active puzzle area clearly.
- Camera size should be chosen from level bounds and aspect ratio, not from a hardcoded one-size-fits-all value.

## Camera Framing Rule

Frame the level based on the logical board bounds.

The camera system should:

- compute the playable grid bounds
- convert those bounds into world-space extents
- choose an orthographic size that keeps the important puzzle area visible
- reserve enough screen space for HUD and safe-area padding

Design goal:

- the board stays readable
- the player remains visible
- the HUD does not overlap critical tiles
- floor visuals should map cleanly to logical tile coordinates instead of being placed freehand
- playable extents should come from authored ground tiles rather than a separate static size box

## UI Canvas

Recommended canvas defaults:

- Render Mode: `Screen Space - Overlay`
- UI Scale Mode: `Scale With Screen Size`
- Reference Resolution: `1080 x 1920`
- Screen Match Mode: `Match Width Or Height`
- Match: `0.5`

Rules:

- Anchor HUD elements to screen edges, not to the puzzle board.
- Keep gameplay-space visuals and screen-space UI separate.
- Use one top HUD band and one bottom control/action band if needed.
- Avoid placing persistent UI over the center of the board.

## Safe Area

All gameplay HUD scenes should include a safe-area container.

Recommended structure:

- `Canvas`
- `SafeAreaRoot`
- `TopBar`
- `BoardFrame` or `CenterContent`
- `BottomBar`
- `OverlayLayer`

Rules:

- Top-level HUD elements must live inside `SafeAreaRoot`.
- Pause buttons, currency, move counter, and similar controls should not sit under notches or status bars.
- Decorative elements may extend further, but interactive UI should remain safe-area compliant.

## Main Gameplay Screen Layout

Use this screen priority order:

1. Puzzle board
2. Move count and essential level state
3. Pause / settings access
4. Optional action buttons
5. Cosmetic or secondary information

Recommended layout:

- Top bar: level label, move counter, pause button
- Center: puzzle board, as large as possible without clipping
- Bottom: on-screen controls only if touch input requires them

Design rules:

- Remove decorative density before reducing tile readability.
- Keep touch targets comfortable for phones.
- Do not crowd the central play space with permanent UI.

## Scene Creation Checklist

When creating a new gameplay scene in Unity:

1. Create a new scene and save it in the appropriate scene folder.
2. Add `Main Camera` and set projection to `Orthographic`.
3. Apply fixed isometric rotation.
4. Add `Canvas` and set `Scale With Screen Size`.
5. Set reference resolution to `1080 x 1920`.
6. Add `SafeAreaRoot` under the canvas.
7. Add top, center, and bottom layout containers.
8. Add `EventSystem` if the scene contains Unity UI interactions.
9. Add the scene composition root or installer for dependency injection.
10. Verify that the board area remains readable in multiple aspect ratios.
11. Group floor visuals near the board root and use `GridGroundTile` on playable floor pieces so movement only allows cells that actually have ground.
12. Put blocking level props under the board root and keep their grid coordinates synced from transform position or explicit inspector values so visible obstacles and movement rules stay in sync across height layers.
13. When converting scene objects to grid cells, derive `X` from world `X`, derive grid height from world `Y` minus any visual offset, and derive `Z` from world `Z`.
14. Keep `GridBoard` focused on `cellSize`, `layerHeight`, and `origin`; let placed ground and obstacle cells define board bounds dynamically.

## Test Aspect Ratios

At minimum, preview these shapes in the Unity Game view:

- `16:9`
- `19.5:9`
- `20:9`
- a narrower small-phone portrait ratio

What to verify:

- no HUD clipping
- no important board area hidden
- no overlap with safe area
- text remains readable
- touch controls remain reachable

## Do Not Do

- Do not use free camera rotation.
- Do not design only for a single phone ratio.
- Do not pin critical UI directly over the board center.
- Do not rely on scene-specific hidden singletons.
- Do not let presentation code become the source of gameplay truth.

## Related Docs

- `README.md`
- `GAME_DESIGN.md`
- `UNITY_STRUCTURE.md`
- `UNITY_BEST_PRACTICES.md`
- `ANDROID_BEST_PRACTICES.md`
- `INPUT_BUFFER_AND_ACTION_FLOW.md`
