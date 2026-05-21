using Godot;

public partial class EnemySpawner : Node2D
{
    [Export] public PackedScene EnemyScene;
    [Export] public int EnemyCount = 3;
    [Export] public float SpawnRadius = 320f;
    [Export] public float WaveDelay = 2f;
    [Export] public float SpawnWarningDuration = 1.5f;

    private enum State { Active, Cooldown, Spawning }
    private State _state = State.Active;
    private float _timer;

    public override void _Ready() => Callable.From(SpawnWave).CallDeferred();

    public override void _Process(double delta)
    {
        if (_state == State.Spawning) return;

        if (_state == State.Cooldown)
        {
            _timer -= (float)delta;
            if (_timer <= 0) SpawnWave();
            return;
        }

        if (GetTree().GetNodesInGroup("enemy").Count == 0)
        {
            _state = State.Cooldown;
            _timer = WaveDelay;
        }
    }

    private void SpawnWave()
    {
        _state = State.Spawning;

        float angleOffset = GD.Randf() * Mathf.Tau;
        var positions = new Vector2[EnemyCount];
        var markers  = new Polygon2D[EnemyCount];

        for (int i = 0; i < EnemyCount; i++)
        {
            float angle = angleOffset + (float)i / EnemyCount * Mathf.Tau;
            positions[i] = GlobalPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * SpawnRadius;

            var marker = new Polygon2D();
            marker.Polygon = new Vector2[] { new(0,-16), new(16,0), new(0,16), new(-16,0) };
            marker.Color = new Color(1f, 0.2f, 0.2f, 0.9f);
            marker.GlobalPosition = positions[i];
            marker.ZIndex = 2;
            GetParent().AddChild(marker);
            markers[i] = marker;

            var tween = marker.CreateTween();
            tween.SetLoops();
            tween.TweenProperty(marker, "modulate:a", 0.1f, 0.25f);
            tween.TweenProperty(marker, "modulate:a", 1.0f, 0.25f);
        }

        GetTree().CreateTimer(SpawnWarningDuration).Timeout += () =>
        {
            for (int i = 0; i < EnemyCount; i++)
            {
                if (IsInstanceValid(markers[i])) markers[i].QueueFree();

                var enemy = EnemyScene.Instantiate<Enemy>();
                enemy.GlobalPosition = positions[i];
                enemy.ZIndex = 1;
                GetParent().AddChild(enemy);
            }
            _state = State.Active;
        };
    }
}
