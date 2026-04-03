# Technical Architecture

## Client

- Unity (C#)
- Android target
- Dependency injection required for app and gameplay architecture

---

## Backend (Supabase)

- PostgreSQL database
- Auth (Google)
- RLS policies
- SQL functions (RPC)
- Optional Edge Functions

---

## Authentication Flow

1. User logs in with Google
2. Supabase returns JWT
3. Client uses JWT for requests
4. RLS enforces permissions

---

## Logic Distribution

### Client

- Gameplay logic
- Movement
- UI
- Scene composition and dependency wiring

### Backend

- Data validation
- Security
- Persistence

---

## Security

- Never expose `service_role` key
- All tables must use RLS

---

## Change Management

- If code changes this architecture, update the related markdown docs in the same task.
- If a change would alter a core documented system, get explicit user approval before implementing it.
