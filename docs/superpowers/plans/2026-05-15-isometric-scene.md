# Isometric Scene Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A playable isometric scene with a 10×10 tile floor and a capsule character that moves with WASD.

**Architecture:** Two C# scripts — `WorldSetup` (extends `TileMapLayer`, builds the `TileSet` and floor procedurally at runtime) and `Player` (extends `CharacterBody2D`, transforms WASD into isometric screen-space directions). Scenes are written as `.tscn` files. No external assets — all visuals are solid-color placeholders.

**Tech Stack:** Godot 4.6, C# (.NET), `TileMapLayer` with isometric tile shape, `CharacterBody2D`, `MoveAndSlide()`

---

### Task 1: Write `Player.cs`

**Files:**
- Create: `scripts/Player.cs`

- [ ] **Step 1: Create the file**

```csharp
using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 150f;

    public override void _PhysicsProcess(double delta)
    {
        var direction = Vector2.Zero;

        if (Input.IsActionPressed("ui_up"))    direction += new Vector2(-1f, -0.5f);
        if (Input.IsActionPressed("ui_down"))  direction += new Vector2(1f,  0.5f);
        if (Input.IsActionPressed("ui_left"))  direction += new Vector2(-1f,  0.5f);
        if (Input.IsActionPressed("ui_right")) direction += new Vector2(1f, -0.5f);

        if (direction != Vector2.Zero)
            direction = direction.Normalized();

        Velocity = direction * Speed;
        MoveAndSlide();
    }
}
```

Save to `scripts/Player.cs`.

- [ ] **Step 2: Commit**

```bash
git add scripts/Player.cs
git commit -m "feat: add isometric player movement script"
```

---

### Task 2: Write `WorldSetup.cs`

**Files:**
- Create: `scripts/WorldSetup.cs`

- [ ] **Step 1: Create the file**

```csharp
using Godot;

public partial class WorldSetup : TileMapLayer
{
    public override void _Ready()
    {
        var image = Image.CreateEmpty(64, 32, false, Image.Format.Rgba8);
        image.Fill(new Color(0.35f, 0.55f, 0.75f));
        var texture = ImageTexture.CreateFromImage(image);

        var source = new TileSetAtlasSource();
        source.Texture = texture;
        source.TextureRegionSize = new Vector2I(64, 32);
        source.CreateTile(new Vector2I(0, 0));

        var tileSet = new TileSet();
        tileSet.TileShape = TileSet.TileShapeEnum.Isometric;
        tileSet.TileSize = new Vector2I(64, 32);
        int sourceId = tileSet.AddSource(source);
        TileSet = tileSet;

        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                SetCell(new Vector2I(x, y), sourceId, new Vector2I(0, 0));
    }
}
```

Save to `scripts/WorldSetup.cs`.

- [ ] **Step 2: Commit**

```bash
git add scripts/WorldSetup.cs
git commit -m "feat: add procedural isometric floor setup script"
```

---

### Task 3: Create `Player.tscn`

**Files:**
- Create: `scenes/Player.tscn`

- [ ] **Step 1: Write the scene file**

```
[gd_scene load_steps=3 format=3]

[ext_resource type="Script" path="res://scripts/Player.cs" id="1"]

[sub_resource type="CapsuleShape2D" id="1"]
radius = 10.0
height = 20.0

[node name="Player" type="CharacterBody2D"]
script = ExtResource("1")

[node name="CollisionShape2D" type="CollisionShape2D" parent="."]
shape = SubResource("1")

[node name="ColorRect" type="ColorRect" parent="."]
offset_left = -10.0
offset_top = -40.0
offset_right = 10.0
offset_bottom = 0.0
color = Color(0.9, 0.35, 0.35, 1)

[node name="Camera2D" type="Camera2D" parent="."]
```

Save to `scenes/Player.tscn`.

- [ ] **Step 2: Commit**

```bash
git add scenes/Player.tscn
git commit -m "feat: add player scene with capsule placeholder and camera"
```

---

### Task 4: Create `Main.tscn`

**Files:**
- Create: `scenes/Main.tscn`

- [ ] **Step 1: Write the scene file**

```
[gd_scene load_steps=3 format=3]

[ext_resource type="PackedScene" path="res://scenes/Player.tscn" id="1"]
[ext_resource type="Script" path="res://scripts/WorldSetup.cs" id="2"]

[node name="Main" type="Node2D"]

[node name="TileMapLayer" type="TileMapLayer" parent="."]
position = Vector2(576, 50)
y_sort_enabled = true
script = ExtResource("2")

[node name="Player" parent="." instance=ExtResource("1")]
position = Vector2(576, 210)
```

Save to `scenes/Main.tscn`.

- [ ] **Step 2: Commit**

```bash
git add scenes/Main.tscn
git commit -m "feat: assemble main scene with floor and player"
```

---

### Task 5: Set `Main.tscn` as the project's main scene

**Files:**
- Modify: `project.godot`

- [ ] **Step 1: Add the main scene entry**

In `project.godot`, under the `[application]` section, add:

```ini
run/main_scene="res://scenes/Main.tscn"
```

The full `[application]` section should look like:

```ini
[application]

config/name="void-shooter-iso"
config/features=PackedStringArray("4.6", "Mobile")
config/icon="res://icon.svg"
run/main_scene="res://scenes/Main.tscn"
```

- [ ] **Step 2: Commit**

```bash
git add project.godot
git commit -m "chore: set Main.tscn as the project main scene"
```

---

### Task 6: Run and verify

- [ ] **Step 1: Run the project via Godot MCP**

Use `mcp__godot__run_project` to launch the project.

Expected result:
- An isometric blue-grey tile floor (10×10 diamond grid) fills the center of the screen
- A red rectangle (the capsule placeholder) sits on the floor
- Pressing W/A/S/D moves the character diagonally along the isometric axes
- The camera follows the player

- [ ] **Step 2: Verify movement directions**

| Key | Expected screen movement |
|-----|--------------------------|
| W   | Up-left                  |
| S   | Down-right               |
| A   | Down-left                |
| D   | Up-right                 |
| W+D | Straight up              |
| S+A | Straight down            |

- [ ] **Step 3: Stop the project**

Use `mcp__godot__stop_project`.

- [ ] **Step 4: Final commit if adjustments were made**

```bash
git add -p
git commit -m "fix: adjust player/floor position for correct isometric layout"
```
