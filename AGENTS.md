# PuzzleGame Agent Context

## Purpose

This repository currently contains the living design and architecture guidelines for a Unity mobile puzzle game targeting Android.

Until the team chooses an official product name, use `PuzzleGame` as the project identifier and refer to it as the working title in documentation.

## Always Apply These Rules

- Treat markdown documentation as a source of truth for gameplay, architecture, backend contracts, and production constraints.
- When code changes affect behavior, architecture, data flow, public APIs, backend contracts, scene flow, or folder structure, update the relevant `.md` files in the same task.
- Do not leave code and documentation out of sync.
- If a requested code change would alter a core function or system already defined in the markdown docs, stop and ask for explicit user approval before implementing it.
- Keep changes incremental and MVP-friendly unless the user asks for a larger redesign.

## Core Systems Requiring Explicit Approval Before Changing

- Grid-based, tile-to-tile puzzle movement
- One input = one full action = one full resolution
- Fixed isometric presentation with no free camera rotation
- Move-count-based scoring and star thresholds
- Token-based level attempt economy
- Supabase-backed authentication, persistence, and server-side validation

## Architecture Expectations

- All gameplay and app code must follow the dependency injection approach used by the project.
- Prefer constructor injection for plain C# classes and explicit installer or composition-root wiring for scene objects.
- Avoid hidden singletons, static mutable state, and direct cross-feature coupling.
- Separate domain logic from Unity-specific `MonoBehaviour` glue.
- Keep gameplay deterministic where possible so level logic is testable outside the Unity runtime.

## Documentation Map

- Product overview: `Skills/README.md`
- Game design: `Skills/GAME_DESIGN.md`
- Technical architecture: `Skills/TECH_ARCHITECTURE.md`
- Unity structure: `Skills/UNITY_STRUCTURE.md`
- Input and action flow: `Skills/INPUT_BUFFER_AND_ACTION_FLOW.md`
- Database schema: `Skills/DATABASE_SCHEMA.md`
- Backend functions: `Skills/DATABASE_FUNCTIONS.md`
- Security rules: `Skills/RLS_POLICIES.md`
- Project best practices: `Skills/UNITY_BEST_PRACTICES.md`, `Skills/SUPABASE_BEST_PRACTICES.md`, `Skills/ANDROID_BEST_PRACTICES.md`, `Skills/MONETIZATION_BEST_PRACTICES.md`

## Naming Guidance

- Keep `PuzzleGame` as the repository and technical working title until the user chooses a final name.
- Avoid inventing brand naming in production-facing copy without approval.
