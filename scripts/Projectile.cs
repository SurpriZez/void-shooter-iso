using Godot;

public partial class Projectile : CharacterBody2D
{
    [Export] public float Speed = 300f;
    [Export] public float Lifetime = 2f;
    [Export] public int Damage = 10;
    public Vector2 Direction;
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
                QueueFree();
                return;
            }
        }
    }
}
