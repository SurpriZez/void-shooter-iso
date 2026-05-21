using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 150f;
    [Export] public float AttackCooldown = 0.3f;
    [Export] public int MeleeDamage = 25;
    [Export] public PackedScene ProjectileScene;

    private enum Weapon { Melee, Ranged }
    private Weapon _weapon = Weapon.Ranged;
    private float _cooldown;

    public override void _PhysicsProcess(double delta)
    {
        _cooldown -= (float)delta;

        var direction = Vector2.Zero;

        if (Input.IsKeyPressed(Key.W)) direction += new Vector2(0f, -1f);
        if (Input.IsKeyPressed(Key.S)) direction += new Vector2(0f,  1f);
        if (Input.IsKeyPressed(Key.A)) direction += new Vector2(-1f, 0f);
        if (Input.IsKeyPressed(Key.D)) direction += new Vector2(1f,  0f);

        if (direction != Vector2.Zero)
            direction = direction.Normalized();

        Velocity = direction * Speed;
        MoveAndSlide();

        if (_weapon == Weapon.Ranged)
        {
            var attackDir = Vector2.Zero;
            if (Input.IsKeyPressed(Key.Up))    attackDir = Vector2.Up;
            if (Input.IsKeyPressed(Key.Down))  attackDir = Vector2.Down;
            if (Input.IsKeyPressed(Key.Left))  attackDir = Vector2.Left;
            if (Input.IsKeyPressed(Key.Right)) attackDir = Vector2.Right;
            foreach (int dev in Input.GetConnectedJoypads())
            {
                if (Input.IsJoyButtonPressed(dev, JoyButton.DpadUp))    attackDir = Vector2.Up;
                if (Input.IsJoyButtonPressed(dev, JoyButton.DpadDown))  attackDir = Vector2.Down;
                if (Input.IsJoyButtonPressed(dev, JoyButton.DpadLeft))  attackDir = Vector2.Left;
                if (Input.IsJoyButtonPressed(dev, JoyButton.DpadRight)) attackDir = Vector2.Right;
            }
            if (attackDir != Vector2.Zero) Attack(attackDir);
        }
    }

    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventKey k && k.Pressed && !k.Echo)
            switch (k.PhysicalKeycode)
            {
                case Key.Up:    Attack(Vector2.Up);    break;
                case Key.Down:  Attack(Vector2.Down);  break;
                case Key.Left:  Attack(Vector2.Left);  break;
                case Key.Right: Attack(Vector2.Right); break;
                case Key.F:     SwitchWeapon();        break;
            }
        else if (ev is InputEventJoypadButton j && j.Pressed)
            switch (j.ButtonIndex)
            {
                case JoyButton.DpadUp:        Attack(Vector2.Up);    break;
                case JoyButton.DpadDown:        Attack(Vector2.Down);  break;
                case JoyButton.DpadLeft:         Attack(Vector2.Left);  break;
                case JoyButton.DpadRight:         Attack(Vector2.Right); break;
                case JoyButton.LeftShoulder: SwitchWeapon();        break;
            }
    }

    private void SwitchWeapon() =>
        _weapon = _weapon == Weapon.Melee ? Weapon.Ranged : Weapon.Melee;

    private void Attack(Vector2 dir)
    {
        if (_cooldown > 0) return;
        _cooldown = AttackCooldown;
        if (_weapon == Weapon.Melee) SpawnMeleeHitbox(dir);
        else SpawnProjectile(dir);
    }

    private void SpawnMeleeHitbox(Vector2 dir)
    {
        var marker = new Polygon2D();
        marker.Polygon = new Vector2[] { new(-14,-14), new(14,-14), new(14,14), new(-14,14) };
        marker.Color = new Color(1f, 0.55f, 0.1f, 0.85f);
        marker.Position = dir * 30;
        AddChild(marker);
        GetTree().CreateTimer(0.15).Timeout += () => marker.QueueFree();

        var shape = new RectangleShape2D();
        shape.Size = new Vector2(28, 28);

        var query = new PhysicsShapeQueryParameters2D();
        query.Shape = shape;
        query.Transform = new Transform2D(0, GlobalPosition + dir * 30);
        query.CollisionMask = 4;

        var hits = GetWorld2D().DirectSpaceState.IntersectShape(query);
        foreach (var hit in hits)
            if (hit["collider"].AsGodotObject() is Enemy enemy)
                enemy.TakeDamage(MeleeDamage);
    }

    private void SpawnProjectile(Vector2 dir)
    {
        var proj = ProjectileScene.Instantiate<Projectile>();
        proj.Direction = dir;
        proj.GlobalPosition = GlobalPosition;
        GetParent().AddChild(proj);
    }
}
