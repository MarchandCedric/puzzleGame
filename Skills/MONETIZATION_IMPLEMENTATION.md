# 💰 Monetization Implementation

## 🎯 Goal

- Ads + IAP system
- Simple and scalable

---

## 📺 Ads

### Interstitial
- Every X levels
- Disabled if remove_ads = true

### Rewarded
- Optional
- Grants tokens

---

## 💳 IAP Products

- remove_ads (non-consumable)
- tokens_small (consumable)
- tokens_medium (consumable)

---

## 🧠 Services

- AdsService
- IAPService
- MonetizationService
- TokenService

---

## 🔄 Flows

### Interstitial
End level → check → show ad

### Rewarded
Click → show → reward tokens

### Purchase
Buy → validate → apply reward

---

## ⚠️ Rules

- No ad spam
- Always allow player choice