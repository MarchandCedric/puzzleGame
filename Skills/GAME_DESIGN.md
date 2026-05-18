# Game Design Document

## Level Structure

- Tile-based grid
- Fixed isometric camera
- No camera rotation

---

## Player

- Non-humanoid robot
- Modular visual design

### Customization

- Colors (body parts)
- Accessories (cosmetic modules)

---

## Objective

- Collect all required collectibles
- Activate the portal
- Reach the active portal
- Optimize move count

---

## Star System

Each level defines:

- `perfect_moves`
- `good_moves`
- `max_moves`

Example:

- `<= 20` -> 3 stars
- `<= 25` -> 2 stars
- `<= 35` -> 1 star

---

## Gameplay Loop

1. Start level
2. Solve puzzle
3. Collect all required collectibles
4. Activate the portal
5. Reach the active portal
6. Calculate score from move count
7. Assign stars
8. Show the level completion UI
9. Let the player continue to the next level, retry, or return to menu

## Level Select Rules

- Each level has `world`, `level`, and star move thresholds
- The player starts with only `1-1` unlocked
- A level unlocks only after the previous level is completed with at least `1` star
- If the player has already completed a level, the best star result and best move count should be shown in level select
- Gameplay scenes should carry their own world and level identity so the menu can target them automatically

---

## Token System

- 1 level attempt = 1 token
- Regeneration:
  - Time-based
  - Rewarded ads
  - Purchases

---

## Retention Design

- Collect all stars
- Unlock worlds
- Optimize solutions

---

## Future Features

- Level editor
- Community levels
- Daily challenges

## Puzzle Elements

- Keys can unlock matching doors
- A consumed key opens one matching door once
- Doors block passage until the player has the required key
- Collectibles are level objectives, not inventory items
- Each level may define a required collectible count; if not explicitly set, the scene's placed `GridCollectible` objects define the required count
- The level portal remains inactive until all required collectibles are collected
- An inactive portal blocks movement like an obstacle
- Moving onto the active portal completes the level immediately after that move resolves and evaluates stars from the move count
- Completing a level should keep the player in the gameplay scene and display score/star UI instead of immediately returning to the menu

---

## Change Rule

- If implementation changes a core gameplay rule in this document, explicit user approval is required before coding it.
