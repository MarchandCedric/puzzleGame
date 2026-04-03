# 🎥 Isometric View Guidelines

## 🎯 Goal

Ensure clarity and readability in a grid-based puzzle game

---

## 📷 Camera

- Fixed rotation
- Orthographic projection (recommended)
- Stable (no constant follow)

---

## 🧱 Grid First

Gameplay is grid-based, not world-position based.

Always use:
- logical grid coordinates
- convert to world position

---

## 👁️ Visibility

- Player must always be visible
- Avoid tall blocking objects
- Limit occlusion

---

## 🎨 Visual Rules

- Minimalistic environment
- Strong contrast
- Clear interactive objects

---

## 🧩 Tile Readability

- Visible grid or subtle separation
- Highlight active tile

---

## 🚫 Avoid

- moving camera constantly
- too much verticality
- excessive visual effects

---

## 🧠 Recommendation

- fixed orthographic camera
- small readable zones
- transitions between zones