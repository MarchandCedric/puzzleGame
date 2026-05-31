# Row Level Security Policies

Policy changes must be documented in this file in the same task as the backend change.

## `profiles`

- `SELECT`: user can read own profile
- `UPDATE`: user can update own profile

---

## `scores`

- `SELECT`: own scores only
- `INSERT`: `user_id = auth.uid()`
- `UPDATE`: `user_id = auth.uid()`

For the MVP, score rows store only the authenticated user's best move count per
stable level GUID. Stars are derived in Unity from local level thresholds.

---

## `levels`

- `SELECT`: only if `is_published = true`

---

## `user_tokens`

- `SELECT/UPDATE`: own row only

---

## `user_customization`

- `SELECT/UPDATE`: own data only
