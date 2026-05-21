using Godot;

public partial class Enemy : CharacterBody2D
{
    [Export] public int MaxHealth = 100;
    private int _health;

    public override void _Ready() => _health = MaxHealth;

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
