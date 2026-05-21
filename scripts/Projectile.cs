using Godot;
using System.Collections.Generic;

public partial class Projectile : CharacterBody2D
{
    [Export] public float Speed = 300f;
    [Export] public float Lifetime = 2f;
    [Export] public int Damage = 10;
    [Export] public float KnockbackForce = 120f;
    [Export] public float KnockbackStun = 0.15f;
    public Vector2 Direction;
    public List<ItemEffect> Effects { get; } = new();
    private float _timeAlive;

    public override void _PhysicsProcess(double delta)
    {
        _timeAlive += (float)delta;
        if (_timeAlive >= Lifetime) { QueueFree(); return; }
        Velocity = Direction * Speed;
        MoveAndSlide();

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            if (GetSlideCollision(i).GetCollider() is Enemy enemy)
            {
                enemy.TakeDamage(Damage);
                enemy.ApplyKnockback(Direction.Normalized(), KnockbackForce, KnockbackStun);
                foreach (var effect in Effects)
                    effect.OnProjectileHit(enemy, this);
                QueueFree();
                return;
            }
        }
    }
}
