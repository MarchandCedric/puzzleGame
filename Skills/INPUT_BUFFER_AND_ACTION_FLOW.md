# Input Buffer And Action Flow

## Principle

1 input = 1 full action = 1 full resolution

---

## System

- `InputReader`
- `InputBuffer` (max size = 2)
- `PlayerActionController`
- `InteractionResolver`
- `CameraZoneController`
- `GameStateMachine`

---

## Game States

- `Idle`
- `ExecutingMove`
- `ResolvingInteractions`
- `CameraTransition`
- `Paused`
- `LevelComplete`

---

## Flow

1. Read input
   Keyboard input and touch HUD buttons must both feed the same move-request path.
2. Validate move
3. Execute animation
4. Update grid position
5. Increment move count
6. Resolve interactions such as key pickup or door unlock
7. Return to `Idle`

---

## Zone Transition

- Triggered by tile
- Clear buffer
- Lock input
- Move camera
- Resume

---

## Rules

- Buffer size = 2
- Ignore input if full
- Clear buffer on transition
- No parallel execution
- If implementation changes this flow, update this file in the same task
- If implementation changes the one-input-one-action core rule, ask for explicit user approval first

---

## Result

- Deterministic gameplay
- Clean logic
- Easy debugging
