using Godot;
using System.Collections.Generic;

public partial class Player : CharacterBody3D
{
    [Export] public float Speed = 6f;
    [Export] public float AttackCooldown = 0.3f;
    [Export] public int MeleeDamage = 25;
    [Export] public int ProjectileDamage = 10;
    [Export] public float KnockbackForce = 12f;
    [Export] public int MaxHealth = 3;
    [Export] public float IframeDuration = 1.0f;
    [Export] public float MeleeRange = 1.8f;
    [Export] public float MeleeAngle = 35f;
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
    private MeshInstance3D _mesh;
    private Camera3D _camera;

    // Screen-space directions mapped to isometric world XZ
    private static readonly Vector3 IsoUp    = new Vector3(-1, 0, -1).Normalized();
    private static readonly Vector3 IsoDown  = new Vector3( 1, 0,  1).Normalized();
    private static readonly Vector3 IsoLeft  = new Vector3(-1, 0,  1).Normalized();
    private static readonly Vector3 IsoRight = new Vector3( 1, 0, -1).Normalized();

    public override void _Ready()
    {
        Health = MaxHealth;
        AddToGroup("player");
        _mesh = GetNode<MeshInstance3D>("Mesh");

        _camera = new Camera3D();
        _camera.Projection = Camera3D.ProjectionType.Orthogonal;
        _camera.Size = 20f;
        AddChild(_camera);
        _camera.Position = new Vector3(10, 12, 10);
        _camera.LookAt(GlobalPosition, Vector3.Up);
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
        MaxHealth        += item.MaxHealthBonus;
        if (item.MaxHealthBonus > 0)
            Health = Mathf.Min(Health + item.MaxHealthBonus, MaxHealth);
        MeleeRange       += item.MeleeRangeBonus;
        MeleeAngle        = Mathf.Min(MeleeAngle + item.MeleeAngleBonus, 89f);

        foreach (var effect in item.Effects)
        {
            _allEffects.Add(effect);
            effect.OnPickup(this);
        }

        SpawnPickupLabel(item);
    }

    private void SpawnPickupLabel(ItemData item)
    {
        var label = new Label3D();
        label.Text = item.ItemName;
        label.Modulate = item.IconColor;
        label.FontSize = 32;
        label.PixelSize = 0.008f;
        label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        label.NoDepthTest = true;
        label.Position = new Vector3(0, 1.5f, 0);
        AddChild(label);

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(label, "position:y", 2.5f, 1.0f)
             .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(label, "modulate:a", 0f, 1.0f).SetDelay(0.4f);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(label.QueueFree));
    }

    public override void _PhysicsProcess(double delta)
    {
        _cooldown -= (float)delta;
        _iframeCooldown -= (float)delta;

        _mesh.Visible = _iframeCooldown <= 0 || Mathf.Sin(_iframeCooldown * 25f) > 0;

        var direction = Vector3.Zero;
        if (Input.IsKeyPressed(Key.W)) direction += IsoUp;
        if (Input.IsKeyPressed(Key.S)) direction += IsoDown;
        if (Input.IsKeyPressed(Key.A)) direction += IsoLeft;
        if (Input.IsKeyPressed(Key.D)) direction += IsoRight;

        if (direction != Vector3.Zero)
            direction = direction.Normalized();

        Velocity = direction * Speed;
        MoveAndSlide();

        _camera.GlobalPosition = GlobalPosition + new Vector3(10, 12, 10);
        _camera.LookAt(GlobalPosition, Vector3.Up);

        if (_weapon == Weapon.Ranged)
        {
            var attackDir = Vector3.Zero;
            if (Input.IsKeyPressed(Key.Up))    attackDir = IsoUp;
            if (Input.IsKeyPressed(Key.Down))  attackDir = IsoDown;
            if (Input.IsKeyPressed(Key.Left))  attackDir = IsoLeft;
            if (Input.IsKeyPressed(Key.Right)) attackDir = IsoRight;
            foreach (int dev in Input.GetConnectedJoypads())
            {
                if (Input.IsJoyButtonPressed(dev, JoyButton.DpadUp))    attackDir = IsoUp;
                if (Input.IsJoyButtonPressed(dev, JoyButton.DpadDown))  attackDir = IsoDown;
                if (Input.IsJoyButtonPressed(dev, JoyButton.DpadLeft))  attackDir = IsoLeft;
                if (Input.IsJoyButtonPressed(dev, JoyButton.DpadRight)) attackDir = IsoRight;
            }
            if (attackDir != Vector3.Zero) Attack(attackDir);
        }

        foreach (var effect in _allEffects)
            effect.OnPlayerProcess(this, delta);
    }

    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventKey k && k.Pressed && !k.Echo)
            switch (k.PhysicalKeycode)
            {
                case Key.Up:    Attack(IsoUp);    break;
                case Key.Down:  Attack(IsoDown);  break;
                case Key.Left:  Attack(IsoLeft);  break;
                case Key.Right: Attack(IsoRight); break;
                case Key.F:     SwitchWeapon();   break;
            }
        else if (ev is InputEventJoypadButton j && j.Pressed)
            switch (j.ButtonIndex)
            {
                case JoyButton.DpadUp:       Attack(IsoUp);    break;
                case JoyButton.DpadDown:     Attack(IsoDown);  break;
                case JoyButton.DpadLeft:     Attack(IsoLeft);  break;
                case JoyButton.DpadRight:    Attack(IsoRight); break;
                case JoyButton.LeftShoulder: SwitchWeapon();   break;
            }
    }

    private void SwitchWeapon() =>
        _weapon = _weapon == Weapon.Melee ? Weapon.Ranged : Weapon.Melee;

    private void Attack(Vector3 dir)
    {
        if (_cooldown > 0) return;
        _cooldown = AttackCooldown;
        if (_weapon == Weapon.Melee) SpawnMeleeHitbox(dir);
        else SpawnProjectile(dir);
    }

    private void SpawnMeleeHitbox(Vector3 dir)
    {
        var shape = new BoxShape3D();
        shape.Size = new Vector3(MeleeRange * 0.56f, 1f, MeleeRange * 0.56f);
        var query = new PhysicsShapeQueryParameters3D();
        query.Shape = shape;
        query.Transform = new Transform3D(Basis.Identity, GlobalPosition + dir * (MeleeRange * 0.6f));
        query.CollisionMask = 4;
        var hits = GetWorld3D().DirectSpaceState.IntersectShape(query);
        foreach (var hit in hits)
            if (hit["collider"].AsGodotObject() is Enemy enemy)
            {
                enemy.TakeDamage(MeleeDamage);
                enemy.ApplyKnockback(dir, KnockbackForce);
                foreach (var effect in _allEffects)
                    effect.OnMeleeHit(enemy, this);
            }

        float baseAngle = Mathf.Atan2(-dir.Z, dir.X);
        float sweepHalf = Mathf.DegToRad(MeleeAngle + 30f);

        var mat = new StandardMaterial3D();
        mat.AlbedoColor = new Color(1f, 0.6f, 0.1f, 0.9f);
        mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        mat.CullMode = BaseMaterial3D.CullModeEnum.Disabled;

        var sweepMesh = new MeshInstance3D();
        sweepMesh.Mesh = MakeSectorMesh(MeleeRange, -Mathf.DegToRad(MeleeAngle), Mathf.DegToRad(MeleeAngle), 8);
        sweepMesh.MaterialOverride = mat;
        sweepMesh.Position = new Vector3(0, 0.05f, 0);
        sweepMesh.Rotation = new Vector3(0, baseAngle - sweepHalf, 0);
        AddChild(sweepMesh);

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(sweepMesh, "rotation:y", baseAngle + sweepHalf, 0.18f)
             .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(mat, "albedo_color:a", 0f, 0.18f)
             .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(sweepMesh.QueueFree));
    }

    private static ArrayMesh MakeSectorMesh(float radius, float startAngle, float endAngle, int segments)
    {
        var vertices = new Vector3[segments + 2];
        vertices[0] = Vector3.Zero;
        for (int i = 0; i <= segments; i++)
        {
            float a = Mathf.Lerp(startAngle, endAngle, i / (float)segments);
            vertices[i + 1] = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
        }

        var indices = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            indices[i * 3]     = 0;
            indices[i * 3 + 1] = i + 1;
            indices[i * 3 + 2] = i + 2;
        }

        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrays[(int)Mesh.ArrayType.Index]  = indices;

        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        return mesh;
    }

    private void SpawnProjectile(Vector3 dir)
    {
        var proj = ProjectileScene.Instantiate<Projectile>();
        proj.Direction = dir;
        proj.Damage = ProjectileDamage;
        proj.KnockbackForce += _knockbackBonus;
        proj.Effects.AddRange(_allEffects);
        foreach (var effect in _allEffects)
            effect.OnProjectileSpawn(proj);
        GetParent().AddChild(proj);
        proj.GlobalPosition = GlobalPosition;
    }
}
