# Android Best Practices For PuzzleGame

## Scope

These practices apply to the Android release target of the Unity client.

## Product Constraints

- Optimize for short play sessions, responsive input, and low-friction resume behavior.
- Assume a wide range of mid-tier Android devices.
- Keep the first playable build lightweight and stable before expanding visual complexity.

## Performance And Stability

- Prioritize stable frame pacing over expensive visual effects.
- Profile on Android hardware, not only in the Unity editor.
- Keep battery, thermal load, and memory usage in mind for long sessions.
- Avoid unnecessary background work while the app is paused or unfocused.

## Input And UX

- Tune touch targets for one-handed portrait or landscape use depending on the final UI layout.
- Keep input latency low and action feedback immediate.
- Ensure pauses, app resumes, interruptions, and focus loss do not corrupt the action queue or token state.

## Resolution And Device Support

- Support multiple aspect ratios cleanly.
- Respect safe areas, display cutouts, and system UI overlays.
- Keep puzzle readability higher priority than decorative density on small screens.

## Networking

- Handle intermittent connections gracefully.
- Avoid making core local puzzle play dependent on a constant network connection unless a specific mode requires it.
- Queue or retry non-destructive backend calls carefully; avoid duplicate reward grants.

## Build And Release Hygiene

- Separate development, staging, and production backend configuration.
- Keep secrets out of the client build.
- Use explicit package and signing configuration per release environment.

## Store Readiness

- Keep ads and purchases compliant with Google Play expectations.
- Make entitlement restoration and ad-removal state resilient across reinstalls and device changes where applicable.

## Documentation Rule

- Any Android-specific build, performance, entitlement, or lifecycle behavior introduced in code should be reflected in the corresponding markdown docs.
