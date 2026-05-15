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
