using Godot;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 150f;
    [Export] public float AttackCooldown = 0.3f;
    [Export] public int MeleeDamage = 25;
    [Export] public int ProjectileDamage = 10;
    [Export] public float KnockbackForce = 350f;
    [Export] public int MaxHealth = 3;
    [Export] public float IframeDuration = 1.0f;
    [Export] public float MeleeRange = 50f;
    [Export] public float MeleeAngle = 25f;
    [Export] public PackedScene ProjectileScene;

    public int Health { get; private set; }
    public IReadOnlyList<ItemData> Items => _items;

    private readonly List<ItemData> _items = new();
    private readonly List<ItemEffect> _allEffects = new();
    private float _knockbackBonus;
    private enum Weapon { Melee, Ranged }
    private Weapon _weapon = Weapon.Ranged;
    private float _cooldown;
    private float _iframeCooldown;

    public override void _Ready()
    {
        Health = MaxHealth;
        AddToGroup("player");
    }

    public void TakeDamage(int amount)
    {
        if (_iframeCooldown > 0) return;
        _iframeCooldown = IframeDuration;
        Health = Mathf.Max(0, Health - amount);
        if (Health == 0)
            GetTree().ReloadCurrentScene();
    }

    public void PickupItem(ItemData item)
    {
        _items.Add(item);
        Speed            += item.SpeedBonus;
        AttackCooldown    = Mathf.Max(0.05f, AttackCooldown + item.AttackCooldownBonus);
        MeleeDamage      += item.MeleeDamageBonus;
        ProjectileDamage += item.ProjectileDamageBonus;
        KnockbackForce   += item.KnockbackBonus;
        _knockbackBonus  += item.KnockbackBonus;
        MeleeRange       += item.MeleeRangeBonus;
        MeleeAngle        = Mathf.Min(MeleeAngle + item.MeleeAngleBonus, 89f);
        MaxHealth        += item.MaxHealthBonus;
        if (item.MaxHealthBonus > 0)
            Health = Mathf.Min(Health + item.MaxHealthBonus, MaxHealth);

        foreach (var effect in item.Effects)
        {
            _allEffects.Add(effect);
            effect.OnPickup(this);
        }

        SpawnPickupLabel(item);
    }

    private void SpawnPickupLabel(ItemData item)
    {
        var label = new Label();
        label.Text = item.ItemName;
        label.AddThemeColorOverride("font_color", item.IconColor);
        label.AddThemeFontSizeOverride("font_size", 18);
        label.Position = new Vector2(-40, -70);
        label.ZIndex = 10;
        AddChild(label);

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(label, "position:y", label.Position.Y - 35f, 1.0f)
             .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(label, "modulate:a", 0f, 1.0f)
             .SetDelay(0.4f);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(label.QueueFree));
    }

    public override void _PhysicsProcess(double delta)
    {
        _cooldown -= (float)delta;
        _iframeCooldown -= (float)delta;

        Modulate = _iframeCooldown > 0
            ? new Color(1, 1, 1, Mathf.Sin(_iframeCooldown * 25f) > 0 ? 1f : 0.15f)
            : Colors.White;

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

        foreach (var effect in _allEffects)
            effect.OnPlayerProcess(this, delta);
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
                case JoyButton.DpadUp:       Attack(Vector2.Up);    break;
                case JoyButton.DpadDown:     Attack(Vector2.Down);  break;
                case JoyButton.DpadLeft:     Attack(Vector2.Left);  break;
                case JoyButton.DpadRight:    Attack(Vector2.Right); break;
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
        var shape = new RectangleShape2D();
        shape.Size = new Vector2(MeleeRange * 0.56f, MeleeRange * 0.56f);
        var query = new PhysicsShapeQueryParameters2D();
        query.Shape = shape;
        query.Transform = new Transform2D(0, GlobalPosition + dir * (MeleeRange * 0.6f));
        query.CollisionMask = 4;
        var hits = GetWorld2D().DirectSpaceState.IntersectShape(query);
        foreach (var hit in hits)
            if (hit["collider"].AsGodotObject() is Enemy enemy)
            {
                enemy.TakeDamage(MeleeDamage);
                enemy.ApplyKnockback(dir, KnockbackForce);
                foreach (var effect in _allEffects)
                    effect.OnMeleeHit(enemy, this);
            }

        float baseAngle = dir.Angle();
        float sweepHalf = Mathf.DegToRad(MeleeAngle + 30f);

        var sweep = new Polygon2D();
        sweep.Polygon = MakeSector(MeleeRange, Mathf.DegToRad(-MeleeAngle), Mathf.DegToRad(MeleeAngle), 8);
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
        proj.Damage = ProjectileDamage;
        proj.GlobalPosition = GlobalPosition;
        proj.KnockbackForce += _knockbackBonus;
        proj.Effects.AddRange(_allEffects);
        foreach (var effect in _allEffects)
            effect.OnProjectileSpawn(proj);
        GetParent().AddChild(proj);
    }
}
