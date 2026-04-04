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

- Reach the exit
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
3. Finish level
4. Calculate score
5. Assign stars
6. Unlock next level

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

---

## Change Rule

- If implementation changes a core gameplay rule in this document, explicit user approval is required before coding it.
