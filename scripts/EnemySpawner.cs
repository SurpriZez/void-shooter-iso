using Godot;

public partial class EnemySpawner : Node2D
{
    [Export] public PackedScene EnemyScene;
    [Export] public int EnemyCount = 3;
    [Export] public float WaveDelay = 2f;
    [Export] public float SpawnWarningDuration = 1.5f;

    private WorldSetup _worldSetup;

    private enum State { Active, Cooldown, Spawning }
    private State _state = State.Active;
    private float _timer;

    public override void _Ready()
    {
        _worldSetup = GetParent().GetNode<WorldSetup>("TileMapLayer");
        Callable.From(SpawnWave).CallDeferred();
    }

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

        var positions = new Vector2[EnemyCount];
        var markers  = new Polygon2D[EnemyCount];

        for (int i = 0; i < EnemyCount; i++)
        {
            positions[i] = GetRandomEdgePosition();

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

    private Vector2 GetRandomEdgePosition()
    {
        int w = _worldSetup.GridWidth;
        int h = _worldSetup.GridHeight;

        Vector2I cell = GD.RandRange(0, 3) switch
        {
            0 => new Vector2I(GD.RandRange(0, w - 1), 0),         // top-right edge
            1 => new Vector2I(w - 1, GD.RandRange(0, h - 1)),     // bottom-right edge
            2 => new Vector2I(GD.RandRange(0, w - 1), h - 1),     // bottom-left edge
            _ => new Vector2I(0, GD.RandRange(0, h - 1)),         // top-left edge
        };

        return _worldSetup.ToGlobal(_worldSetup.MapToLocal(cell));
    }
}
