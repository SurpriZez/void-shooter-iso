using Godot;
using System.Collections.Generic;

public partial class Enemy : CharacterBody2D
{
    [Export] public int MaxHealth = 100;
    [Export] public Color BodyColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    [Export] public float Speed = 60f;
    [Export] public int ContactDamage = 1;
    [Export] public float StunDuration = 0.4f;

    private int _health;
    private Player _player;
    private float _stunTimer;
    private Vector2 _knockbackVelocity;
    private readonly List<StatusEffect> _statusEffects = new();

    public override void _Ready()
    {
        _health = MaxHealth;
        GetNode<ColorRect>("ColorRect").Color = BodyColor;
        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        AddToGroup("enemy");
    }

    public void ApplyKnockback(Vector2 direction, float force, float stunDuration = -1f)
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

        Modulate = HasStatus("burn") ? new Color(1f, 0.5f, 0.15f) : Colors.White;

        if (_stunTimer > 0)
        {
            _stunTimer -= (float)delta;
            _knockbackVelocity = _knockbackVelocity.Lerp(Vector2.Zero, 10f * (float)delta);
            Velocity = _knockbackVelocity;
        }
        else
        {
            Velocity = (_player.GlobalPosition - GlobalPosition).Normalized() * Speed;
        }

        MoveAndSlide();

        for (int i = 0; i < GetSlideCollisionCount(); i++)
            if (GetSlideCollision(i).GetCollider() is Player player)
                player.TakeDamage(ContactDamage);
    }

    public void TakeDamage(int amount)
    {
        _health -= amount;
        SpawnDamageLabel(amount, new Color(1f, 0.9f, 0.1f), 20, 40f);
        if (_health <= 0) QueueFree();
    }

    public void TakeDotDamage(int amount)
    {
        _health -= amount;
        SpawnDamageLabel(amount, new Color(1f, 0.5f, 0.1f), 13, 22f);
        if (_health <= 0) QueueFree();
    }

    private void SpawnDamageLabel(int amount, Color color, int fontSize, float riseY)
    {
        var label = new Label();
        label.Text = amount.ToString();
        label.Position = new Vector2(GD.RandRange(-15, 15), -55);
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.ZIndex = 10;
        AddChild(label);

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(label, "position", label.Position + new Vector2(0, -riseY), 0.7f);
        tween.TweenProperty(label, "modulate:a", 0f, 0.7f);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(label.QueueFree));
    }
}
