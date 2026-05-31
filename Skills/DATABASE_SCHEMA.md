# Database Schema (Supabase)

Schema changes must be reflected in this file in the same task as the implementation change.

## `profiles`

Optional after the MVP. Supabase Auth already owns authentication users in
`auth.users`; create `profiles` only when the game needs public profile data.

- `id` (uuid, PK, `auth.users.id`)
- `username` (text)
- `created_at` (timestamp)
- `avatar_config` (jsonb)

---

## `levels`

Optional for the first score-saving MVP because Unity-authored level data contains
the stable level GUID, scene routing data, and star thresholds. Add this table when
the backend needs authoritative level metadata, remote level catalogs, publishing
state, or server-side score validation.

- `id` (uuid)
- `name` (text)
- `data` (jsonb)
- `perfect_moves` (int)
- `good_moves` (int)
- `max_moves` (int)
- `is_published` (bool)

---

## `scores`

- `user_id` (uuid, FK to `auth.users.id`)
- `level_id` (uuid)
- `best_move_count` (int)
- `updated_at` (timestamp)

Constraints:

- Primary key: (`user_id`, `level_id`)
- `best_move_count > 0`

Notes:

- `level_id` is the stable GUID authored in Unity for each level.
- Do not store stars for the MVP. Stars are derived from `best_move_count` and the
  current Unity-authored level thresholds.
- When local and Supabase scores conflict, keep the lower move count. Use
  `updated_at` for sync bookkeeping and diagnostics, not as the only conflict
  resolver.

---

## `user_tokens`

- `user_id` (uuid)
- `tokens` (int)
- `last_regen` (timestamp)

---

## `user_customization`

- `user_id` (uuid)
- `colors` (jsonb)
- `accessories` (jsonb)

---

## `level_submissions`

- `id` (uuid)
- `user_id` (uuid)
- `level_data` (jsonb)
- `status` (pending, approved, rejected)
