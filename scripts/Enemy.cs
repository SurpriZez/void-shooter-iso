using Godot;

public partial class Enemy : CharacterBody2D
{
    [Export] public int MaxHealth = 100;
    [Export] public Color BodyColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    [Export] public float Speed = 60f;
    [Export] public int ContactDamage = 1;

    private int _health;
    private Player _player;

    public override void _Ready()
    {
        _health = MaxHealth;
        GetNode<ColorRect>("ColorRect").Color = BodyColor;
        _player = GetTree().GetFirstNodeInGroup("player") as Player;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_player == null || !IsInstanceValid(_player)) return;

        var dir = (_player.GlobalPosition - GlobalPosition).Normalized();
        Velocity = dir * Speed;
        MoveAndSlide();

        for (int i = 0; i < GetSlideCollisionCount(); i++)
            if (GetSlideCollision(i).GetCollider() is Player player)
                player.TakeDamage(ContactDamage);
    }

    public void TakeDamage(int amount)
    {
        _health -= amount;
        SpawnDamageText(amount);
        if (_health <= 0) QueueFree();
    }

    private void SpawnDamageText(int amount)
    {
        var label = new Label();
        label.Text = amount.ToString();
        label.Position = new Vector2(-15, -60);
        label.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.1f));
        label.AddThemeFontSizeOverride("font_size", 20);
        label.ZIndex = 10;
        AddChild(label);

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(label, "position", label.Position + new Vector2(0, -40), 0.8f);
        tween.TweenProperty(label, "modulate:a", 0f, 0.8f);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(label.QueueFree));
    }
}
