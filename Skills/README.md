# Puzzle Game (Working Title)

## Concept

A mobile puzzle game inspired by Chip's Challenge, built in Unity (C#) for Android, using a fixed isometric view.

The player controls a small modular robot navigating grid-based levels.

Goal: reach the exit using the minimum number of moves.

---

## Core Gameplay

- Grid-based movement (tile-to-tile)
- One input = one action
- Movement types:
  - Move (4 directions)
  - Interaction (optional)
- Move count defines score

---

## Scoring System

Each level defines:
- Perfect score -> 3 stars
- Good score -> 2 stars
- Minimum score -> 1 star

Score is based on number of actions (moves).

---

## Features

- Level progression
- Star rating system
- Robot customization (colors + accessories)
- Token (energy) system
- Ads (interstitial + rewarded)
- In-app purchases:
  - Remove ads
  - Buy tokens

---

## Backend

- Supabase (PostgreSQL)
- Google authentication
- Row Level Security (RLS)
- SQL functions (RPC)

---

## Goal

Build a clean, scalable MVP with strong gameplay and extensible architecture.

---

## Documentation Rules

- This repository is a living specification: when implementation changes behavior or architecture, the related markdown files must be updated in the same task.
- Any change to a core documented gameplay or backend system requires explicit user approval before implementation.
- All future code should follow the project's dependency injection approach.

---

## Recommended Reading Order

- `../AGENTS.md`
- `TECH_ARCHITECTURE.md`
- `UNITY_STRUCTURE.md`
- `UNITY_SCENE_SETUP.md`
- `GAME_DESIGN.md`
- `INPUT_BUFFER_AND_ACTION_FLOW.md`
- `DATABASE_SCHEMA.md`
- `DATABASE_FUNCTIONS.md`
- `RLS_POLICIES.md`
- `UNITY_BEST_PRACTICES.md`
- `SUPABASE_BEST_PRACTICES.md`
- `ANDROID_BEST_PRACTICES.md`
- `MONETIZATION_BEST_PRACTICES.md`
