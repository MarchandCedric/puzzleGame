# Supabase RPC Functions

Changes to gameplay-critical RPC behavior require both documentation updates and explicit user approval when they alter a core documented system.

## `submit_score(level_id, move_count)`

### Logic

1. Fetch level thresholds
2. Calculate stars
3. Compare with best score
4. Insert or update

---

## `consume_token()`

### Logic

- Check `tokens > 0`
- Decrement

---

## `regenerate_tokens()`

### Logic

- Compute elapsed time
- Add tokens

---

## `update_customization(colors, accessories)`

### Logic

- Update `user_customization`

---

## `submit_level(level_data)`

### Logic

- Insert into `level_submissions`
- Set `status = pending`
