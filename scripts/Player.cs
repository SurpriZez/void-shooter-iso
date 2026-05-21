using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 150f;
    [Export] public float AttackCooldown = 0.3f;
    [Export] public int MeleeDamage = 25;
    [Export] public int MaxHealth = 3;
    [Export] public PackedScene ProjectileScene;

    public int Health { get; private set; }

    private enum Weapon { Melee, Ranged }
    private Weapon _weapon = Weapon.Ranged;
    private float _cooldown;

    public override void _Ready()
    {
        Health = MaxHealth;
        AddToGroup("player");
    }

    public void TakeDamage(int amount)
    {
        Health = Mathf.Max(0, Health - amount);
        if (Health == 0)
            GetTree().ReloadCurrentScene();
    }

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
        // hit detection — instant shape query
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

        // sweep visual
        float baseAngle = dir.Angle();
        float sweepHalf = Mathf.DegToRad(55f);

        var sweep = new Polygon2D();
        sweep.Polygon = MakeSector(50f, Mathf.DegToRad(-25f), Mathf.DegToRad(25f), 8);
        sweep.Color = new Color(1f, 0.6f, 0.1f, 0.9f);
        sweep.Rotation = baseAngle - sweepHalf;
        sweep.ZIndex = 2;
        AddChild(sweep);

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(sweep, "rotation", baseAngle + sweepHalf, 0.18f)
             .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(sweep, "modulate:a", 0f, 0.18f)
             .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(sweep.QueueFree));
    }

    private static Vector2[] MakeSector(float radius, float startAngle, float endAngle, int segments)
    {
        var pts = new Vector2[segments + 2];
        pts[0] = Vector2.Zero;
        for (int i = 0; i <= segments; i++)
        {
            float a = Mathf.Lerp(startAngle, endAngle, i / (float)segments);
            pts[i + 1] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }
        return pts;
    }

    private void SpawnProjectile(Vector2 dir)
    {
        var proj = ProjectileScene.Instantiate<Projectile>();
        proj.Direction = dir;
        proj.GlobalPosition = GlobalPosition;
        GetParent().AddChild(proj);
    }
}
