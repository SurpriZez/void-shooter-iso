using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 150f;

    public override void _PhysicsProcess(double delta)
    {
        var direction = Vector2.Zero;

        if (Input.IsKeyPressed(Key.W)) direction += new Vector2(-1f, -0.5f);
        if (Input.IsKeyPressed(Key.S)) direction += new Vector2(1f,  0.5f);
        if (Input.IsKeyPressed(Key.A)) direction += new Vector2(-1f,  0.5f);
        if (Input.IsKeyPressed(Key.D)) direction += new Vector2(1f, -0.5f);

        if (direction != Vector2.Zero)
            direction = direction.Normalized();

        Velocity = direction * Speed;
        MoveAndSlide();
    }
}
