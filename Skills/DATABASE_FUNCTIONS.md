# Supabase RPC Functions

Changes to gameplay-critical RPC behavior require both documentation updates and explicit user approval when they alter a core documented system.

## MVP Score Saving

For the first score-saving MVP, Unity-authored level data is the source of truth
for level identity and star thresholds. Supabase stores only each authenticated
player's best move count per stable level GUID.

Score submission may be implemented with an upsert or a small RPC. In either case,
the stored result must keep the lower move count for (`user_id`, `level_id`).

## `submit_score(level_id, move_count)`

Optional MVP RPC. Use this when the client integration is cleaner with one call or
when score writes need server-side conflict handling before full level validation
exists.

### MVP Logic

1. Use `auth.uid()` as the user id
2. Validate `move_count > 0`
3. Insert a row when none exists
4. Update `best_move_count` only when the submitted move count is lower
5. Return the saved best move count and `updated_at`

### Future Authoritative Logic

1. Fetch level thresholds
2. Calculate stars
3. Compare with best score
4. Insert or update

Future server-authoritative star calculation requires Supabase to store or fetch
official level thresholds. Until then, stars remain derived in Unity from the saved
best move count and local level thresholds.

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
