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
- Controls should be screen-relative for clarity in puzzle scenes with a fixed isometric camera
- Pressing a direction should move the player toward the tile that appears in that direction on screen

---

## 🚫 Avoid

- moving camera constantly
- too much verticality
- excessive visual effects
- controls that require players to mentally rotate the board before predicting the next tile

---

## 🧠 Recommendation

- fixed orthographic camera
- small readable zones
- transitions between zones
