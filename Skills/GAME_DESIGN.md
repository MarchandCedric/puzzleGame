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
- a stable level GUID used for persistence

Example:

- `<= 20` -> 3 stars
- `<= 25` -> 2 stars
- `<= 35` -> 1 star

For the MVP, these thresholds live in Unity-authored level data. Saved cloud score
data should store the best move count only; the displayed star result is derived
from the saved move count and the level's current thresholds. This keeps level
tuning flexible after publishing because updated thresholds can be reflected
without rewriting every saved score row.

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

- Each level has a stable GUID, `world`, `level`, and star move thresholds
- Each level catalog entry can define a display title for menu and level-select UI
- The player starts with only `1-1` unlocked
- A level unlocks only after the previous level is completed with at least `1` star
- The highest available level is the first locked step after the last sequential level completed with at least `1` star
- If the player has already completed a level, the best star result and best move count should be shown in level select
- Local MVP progress is stored outside `LevelCatalog`; the catalog describes levels, while local player best moves and stars are stored in PlayerPrefs-compatible progress keys
- Local MVP progress also tracks total attempts per level. An attempt is recorded when a gameplay scene starts, so starts, restarts, selected-level loads, and next-level loads are counted consistently.
- Gameplay scenes should carry their own world and level identity so the menu can target them automatically
- The stable GUID, not the readable `world-level` label, is the persistence identity
- If local and cloud scores differ, the lower move count is the winning best score

---

## Token System

- 1 level attempt = 1 token
- Local MVP:
  - Starting a level consumes 1 attempt
  - Restarting a level consumes 1 attempt
  - Continuing from a completed level to the next level consumes 1 attempt
  - Free players can use 10 attempts before a cooldown starts
  - Cooldown lasts 10 minutes
  - Attempt count and cooldown end time must persist across app restarts and work offline
  - A player with a locally known active subscription bypasses the limit and resets the local counter
- Future online version:
  - Supabase should become authoritative for online users when the economy needs stronger validation
  - The client should keep the same local gate as an offline fallback
  - Offline cooldowns are a soft limit and can be bypassed by determined users changing local device state
- Future regeneration sources:
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
- The player can hold multiple keys of the same color
- A consumed key opens one matching door once
- Doors block passage until the player has the required key
- Collectibles are level objectives, not inventory items
- Each level may define a required collectible count; if not explicitly set, the scene's placed `GridCollectible` objects define the required count
- The level portal remains inactive until all required collectibles are collected
- An inactive portal blocks movement like an obstacle
- Moving onto the active portal locks gameplay, evaluates stars from the move count, may play a short victory transition, and then shows the level completion UI
- Completing a level should keep the player in the gameplay scene and display score/star UI instead of immediately returning to the menu

---

## Change Rule

- If implementation changes a core gameplay rule in this document, explicit user approval is required before coding it.
