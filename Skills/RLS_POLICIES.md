# Row Level Security Policies

Policy changes must be documented in this file in the same task as the backend change.

## `profiles`

- `SELECT`: user can read own profile
- `UPDATE`: user can update own profile

---

## `scores`

- `INSERT`: `user_id = auth.uid()`
- `SELECT`: own scores only

---

## `levels`

- `SELECT`: only if `is_published = true`

---

## `user_tokens`

- `SELECT/UPDATE`: own row only

---

## `user_customization`

- `SELECT/UPDATE`: own data only
