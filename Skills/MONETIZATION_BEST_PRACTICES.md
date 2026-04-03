# Monetization Best Practices For PuzzleGame

## Scope

These practices apply to rewarded ads, interstitials, purchases, and the token economy for PuzzleGame.

## Player Trust

- Monetization must never make puzzle outcomes feel unfair or manipulated.
- Rewarded ads should always be opt-in.
- Interstitials should be frequency-capped and easy to disable through the documented purchase path.
- Communicate rewards, costs, and cooldowns clearly.

## Economy Rules

- Token spending and regeneration must be backend-authoritative when they affect persistent progression.
- Purchase grants and rewarded-ad rewards must be idempotent.
- Keep paid entitlements separate from soft-currency balances when possible.

## Integration Architecture

- Wrap ad network and billing SDKs behind injected interfaces.
- Keep gameplay systems unaware of vendor SDK details.
- Route all monetization outcomes through dedicated services such as `AdsService`, `IAPService`, `MonetizationService`, and `TokenService`.

## UX Guardrails

- Never interrupt a critical puzzle interaction with an ad.
- Prefer natural presentation points such as level end or explicit reward prompts.
- Preserve user progress before showing ads or purchase flows.

## Analytics And Validation

- Track impressions, completions, purchase attempts, purchase confirmations, and entitlement application.
- Validate purchase state securely before granting permanent benefits.
- Investigate mismatches between claimed rewards and stored balances.

## Documentation Rule

- Any monetization flow change must update the markdown docs because it affects player economy, backend validation, and release behavior.
