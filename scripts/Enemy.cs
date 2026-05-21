using Godot;
using System.Collections.Generic;

public partial class Enemy : CharacterBody3D
{
    [Export] public int MaxHealth = 100;
    [Export] public Color BodyColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    [Export] public float Speed = 3f;
    [Export] public int ContactDamage = 1;
    [Export] public float StunDuration = 0.4f;

    private int _health;
    private Player _player;
    private float _stunTimer;
    private Vector3 _knockbackVelocity;
    private readonly List<StatusEffect> _statusEffects = new();
    private StandardMaterial3D _mat;

    public override void _Ready()
    {
        _health = MaxHealth;
        _mat = new StandardMaterial3D();
        _mat.AlbedoColor = BodyColor;
        GetNode<MeshInstance3D>("Mesh").MaterialOverride = _mat;
        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        AddToGroup("enemy");
    }

    public void ApplyKnockback(Vector3 direction, float force, float stunDuration = -1f)
    {
        _knockbackVelocity = direction * force;
        _stunTimer = stunDuration < 0 ? StunDuration : stunDuration;
    }

    public void ApplyStatus(StatusEffect effect)
    {
        foreach (var existing in _statusEffects)
        {
            if (existing.Id == effect.Id)
            {
                existing.OnStack(effect);
                return;
            }
        }
        _statusEffects.Add(effect);
    }

    public bool HasStatus(string id)
    {
        foreach (var s in _statusEffects)
            if (s.Id == id) return true;
        return false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_player == null || !IsInstanceValid(_player)) return;

        for (int i = _statusEffects.Count - 1; i >= 0; i--)
        {
            if (_health <= 0) break;
            if (_statusEffects[i].Update(this, (float)delta))
                _statusEffects.RemoveAt(i);
        }

        _mat.AlbedoColor = HasStatus("burn") ? new Color(1f, 0.5f, 0.15f) : BodyColor;

        if (_stunTimer > 0)
        {
            _stunTimer -= (float)delta;
            _knockbackVelocity = _knockbackVelocity.Lerp(Vector3.Zero, 10f * (float)delta);
            Velocity = _knockbackVelocity;
        }
        else
        {
            var toPlayer = _player.GlobalPosition - GlobalPosition;
            toPlayer.Y = 0;
            Velocity = toPlayer.Normalized() * Speed;
        }

        MoveAndSlide();

        for (int i = 0; i < GetSlideCollisionCount(); i++)
            if (GetSlideCollision(i).GetCollider() is Player player)
                player.TakeDamage(ContactDamage);
    }

    public void TakeDamage(int amount)
    {
        _health -= amount;
        SpawnDamageLabel(amount, new Color(1f, 0.9f, 0.1f), 28, 0.8f);
        if (_health <= 0) QueueFree();
    }

    public void TakeDotDamage(int amount)
    {
        _health -= amount;
        SpawnDamageLabel(amount, new Color(1f, 0.5f, 0.1f), 18, 0.4f);
        if (_health <= 0) QueueFree();
    }

    private void SpawnDamageLabel(int amount, Color color, int fontSize, float riseY)
    {
        var label = new Label3D();
        label.Text = amount.ToString();
        label.Modulate = color;
        label.FontSize = fontSize;
        label.PixelSize = 0.008f;
        label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        label.NoDepthTest = true;
        label.Position = new Vector3((float)GD.RandRange(-0.3f, 0.3f), 1.2f, 0);
        AddChild(label);

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(label, "position:y", label.Position.Y + riseY, 0.7f);
        tween.TweenProperty(label, "modulate:a", 0f, 0.7f);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(label.QueueFree));
    }
}
