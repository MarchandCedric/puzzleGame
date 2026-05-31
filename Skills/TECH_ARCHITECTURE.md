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

For the Unity MVP, the main menu starts in an unauthenticated state and shows only
login UI. The login button opens Supabase OAuth in the system browser. Supabase
redirects back to the app through the configured mobile deep link, and Unity stores
the returned session tokens for later score requests.

---

## Logic Distribution

### Client

- Gameplay logic
- Movement
- UI
- Scene composition and dependency wiring
- MVP level metadata, including stable level GUIDs and star thresholds
- Star display derived from saved best move count plus local level thresholds

### Backend

- Data validation
- Security
- Persistence
- MVP cloud score storage: authenticated best move count per level GUID

For the score-saving MVP, Unity remains the source of truth for level thresholds
while levels are still being tuned. Supabase Auth identifies the player, and
Supabase stores best move counts. Later, if score validation must become fully
authoritative, official level metadata and star thresholds should move into
Supabase-backed data or a backend validation path.

---

## Security

- Never expose `service_role` key
- All tables must use RLS

---

## Change Management

- If code changes this architecture, update the related markdown docs in the same task.
- If a change would alter a core documented system, get explicit user approval before implementing it.
