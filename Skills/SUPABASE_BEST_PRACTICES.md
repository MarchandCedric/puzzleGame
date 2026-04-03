# Supabase And Database Best Practices For PuzzleGame

## Scope

These practices apply to Supabase, PostgreSQL, authentication, and backend data contracts for PuzzleGame.

## Security Baseline

- Never expose the `service_role` key in the Unity client.
- Enable RLS on every user-facing table.
- Default to deny, then add explicit policies for required operations.
- Keep authoritative validation for rewards, score submission, and token consumption on the backend.

## Schema Design

- Use UUIDs consistently for user-owned and level-related entities.
- Add explicit primary keys, foreign keys, unique constraints, and check constraints instead of relying only on client logic.
- Keep mutable player state in narrow tables with one clear owner per row.
- Use `jsonb` only when the shape is intentionally flexible; prefer typed columns for stable gameplay-critical data.

## RPC And Server Logic

- Put score calculation, token consumption, and reward grants behind RPC functions when they affect progression or economy.
- Make functions idempotent or concurrency-safe when duplicate client requests are possible.
- Return structured results from RPCs so the Unity client can react without extra round-trips.
- Version function behavior carefully; changing gameplay contracts requires documentation updates and user approval when it alters a core documented system.

## Authentication

- Use Google sign-in as the identity-provider entry point and rely on Supabase JWT validation for backend access control.
- Base user ownership checks on `auth.uid()`.
- Avoid client-writable fields that duplicate authenticated identity.

## Persistence Rules

- Treat level definitions and publication state as controlled content.
- Treat player best scores, tokens, purchases, and customization as user-scoped data.
- Keep monetization entitlements auditable and separate from temporary balances where possible.

## Migrations And Change Management

- Make all database changes through explicit migrations.
- Document schema, RPC, and policy changes in the markdown docs during the same task.
- Do not ship destructive schema changes without a rollback plan.

## Performance

- Index foreign keys and common lookup paths such as `user_id`, `level_id`, and publication flags.
- Keep score submission queries small and predictable.
- Use pagination or bounded queries for history and leaderboard-style views if introduced later.

## Observability

- Log backend failures in a way that distinguishes auth, validation, and transient infrastructure issues.
- Prefer backend error messages that are safe for clients and useful for debugging.
