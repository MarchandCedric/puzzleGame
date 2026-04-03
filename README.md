# PuzzleGame

`PuzzleGame` is the current working title for this Unity mobile puzzle game project.

The design and architecture source of truth lives in the markdown files under [`Skills/`](c:\Users\cedlf\Documents\Projets%20Ced\PuzzleGame\Skills\README.md).

## Local Secrets

Never commit API keys, access tokens, credentials, signing files, or personal information.

Use local-only files for secrets:

- Root environment files: `.env`, `.env.local`
- Supabase local environment files: `supabase/.env`, `supabase/functions/.env`
- Android signing files: `*.keystore`, `*.jks`
- Private key or certificate files: `*.key`, `*.pem`, `*.p12`

These files are ignored by git through [`.gitignore`](c:\Users\cedlf\Documents\Projets%20Ced\PuzzleGame\.gitignore).

Safe-to-commit examples:

- `.env.example`
- `.env.sample`
- Supabase migrations
- Supabase config meant for the team

Recommended workflow:

1. Keep real secrets only in local ignored env files.
2. Commit template files with placeholder values when the team needs setup guidance.
3. Rotate any secret immediately if it was ever committed or shared publicly.
