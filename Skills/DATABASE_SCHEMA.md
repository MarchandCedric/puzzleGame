# Database Schema (Supabase)

Schema changes must be reflected in this file in the same task as the implementation change.

## `profiles`

- `id` (uuid, PK, `auth.users.id`)
- `username` (text)
- `created_at` (timestamp)
- `avatar_config` (jsonb)

---

## `levels`

- `id` (uuid)
- `name` (text)
- `data` (jsonb)
- `perfect_moves` (int)
- `good_moves` (int)
- `max_moves` (int)
- `is_published` (bool)

---

## `scores`

- `id` (uuid)
- `user_id` (uuid)
- `level_id` (uuid)
- `move_count` (int)
- `stars` (int)
- `created_at` (timestamp)

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
