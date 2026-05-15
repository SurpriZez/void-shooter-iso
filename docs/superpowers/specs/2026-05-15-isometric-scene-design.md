# Isometric Scene Design

**Date:** 2026-05-15
**Project:** void-shooter-iso (Godot 4.6, C#, Mobile renderer, Jolt Physics)

## Goal

A basic playable isometric scene — a placeholder floor the player can walk around on — as the starting foundation for the game.

## Scene Structure

```
Main (Node2D)
├── TileMapLayer          ← isometric floor, Y-sort enabled
└── Player (CharacterBody2D)
    ├── CollisionShape2D  ← CapsuleShape2D
    ├── ColorRect         ← placeholder visual (tall rectangle)
    └── Camera2D          ← follows player
```

## TileMapLayer

- Tile shape: **Isometric**
- Tile size: **64 × 32 px**
- One placeholder tile: solid color rectangle
- Floor layout: **10 × 10** tiles filled at start
- Y-sort enabled on the layer for correct draw order

## Player

- Node type: `CharacterBody2D`
- Collision: `CapsuleShape2D`
- Visual: `ColorRect` (tall rectangle placeholder, no sprite)
- Script: `Player.cs`

## Movement

WASD input is transformed into isometric screen-space directions:

| Key | Direction     | Vector          |
|-----|---------------|-----------------|
| W   | up-left       | (−1, −0.5) norm |
| S   | down-right    | (+1, +0.5) norm |
| A   | down-left     | (−1, +0.5) norm |
| D   | up-right      | (+1, −0.5) norm |

Combined directions (e.g. W+D) are summed and normalized before applying speed. Movement uses `MoveAndSlide()`.

## Camera

- Node type: `Camera2D`, child of Player
- Follows the player automatically by being a child node
- No zoom, limits, or smoothing for now

## Out of Scope

- Pixel art settings (nearest-neighbor filtering, integer scaling) — deferred
- Animations
- Enemies or combat
- Collision on tile edges
