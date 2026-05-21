using Godot;
using System.Collections.Generic;

public partial class EnemySpawner : Node3D
{
    [Export] public PackedScene EnemyScene;
    [Export] public PackedScene ItemPedestalScene;
    [Export] public int EnemyCount = 3;
    [Export] public float WaveDelay = 2f;
    [Export] public float SpawnWarningDuration = 1.5f;

    private ItemData[] _pool;
    private WorldSetup _worldSetup;

    private enum State { Active, ItemPhase, Cooldown, Spawning }
    private State _state = State.Active;
    private float _timer;

    public override void _Ready()
    {
        _worldSetup = GetParent().GetNode<WorldSetup>("WorldSetup");
        Callable.From(SpawnWave).CallDeferred();
    }

    public override void _Process(double delta)
    {
        if (_state == State.Spawning || _state == State.ItemPhase) return;

        if (_state == State.Cooldown)
        {
            _timer -= (float)delta;
            if (_timer <= 0) SpawnWave();
            return;
        }

        if (GetTree().GetNodesInGroup("enemy").Count == 0)
            SpawnItemPedestal();
    }

    private ItemData[] GetPool()
    {
        if (_pool != null) return _pool;
        var items = new List<ItemData>();
        using var dir = DirAccess.Open("res://items");
        if (dir != null)
        {
            dir.ListDirBegin();
            string name;
            while ((name = dir.GetNext()) != "")
            {
                if (!dir.CurrentIsDir() && name.EndsWith(".tres"))
                {
                    var item = GD.Load<ItemData>("res://items/" + name);
                    if (item != null) items.Add(item);
                }
            }
            dir.ListDirEnd();
        }
        return _pool = items.ToArray();
    }

    private void SpawnItemPedestal()
    {
        _state = State.ItemPhase;
        var pool = GetPool();
        var item = pool.Length > 0 ? pool[GD.RandRange(0, pool.Length - 1)] : null;
        if (item == null) { _state = State.Cooldown; _timer = WaveDelay; return; }
        var scene = ItemPedestalScene ?? GD.Load<PackedScene>("res://scenes/ItemPedestal.tscn");
        var pedestal = scene.Instantiate<ItemPedestal>();
        pedestal.Initialize(item, () =>
        {
            _state = State.Cooldown;
            _timer = WaveDelay;
        });
        var centerCell = new Vector2I(_worldSetup.GridWidth / 2, _worldSetup.GridHeight / 2);
        pedestal.GlobalPosition = _worldSetup.GridToWorld(centerCell);
        GetParent().AddChild(pedestal);
    }

    private void SpawnWave()
    {
        _state = State.Spawning;

        var positions = new Vector3[EnemyCount];
        var markers  = new MeshInstance3D[EnemyCount];

        for (int i = 0; i < EnemyCount; i++)
        {
            positions[i] = GetRandomEdgePosition();

            var marker = new MeshInstance3D();
            var quad = new QuadMesh();
            quad.Size = new Vector2(0.7f, 0.7f);
            marker.Mesh = quad;
            marker.RotationDegrees = new Vector3(-90, 45, 0);
            var mat = new StandardMaterial3D();
            mat.AlbedoColor = new Color(1f, 0.2f, 0.2f, 0.9f);
            mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            mat.ShadingMode  = BaseMaterial3D.ShadingModeEnum.Unshaded;
            marker.MaterialOverride = mat;
            marker.GlobalPosition = positions[i] + Vector3.Up * 0.01f;
            GetParent().AddChild(marker);
            markers[i] = marker;

            var tween = marker.CreateTween();
            tween.SetLoops();
            tween.TweenProperty(mat, "albedo_color:a", 0.1f, 0.25f);
            tween.TweenProperty(mat, "albedo_color:a", 1.0f, 0.25f);
        }

        GetTree().CreateTimer(SpawnWarningDuration).Timeout += () =>
        {
            for (int i = 0; i < EnemyCount; i++)
            {
                if (IsInstanceValid(markers[i])) markers[i].QueueFree();
                var enemy = EnemyScene.Instantiate<Enemy>();
                enemy.GlobalPosition = positions[i];
                GetParent().AddChild(enemy);
            }
            _state = State.Active;
        };
    }

    private Vector3 GetRandomEdgePosition()
    {
        int w = _worldSetup.GridWidth;
        int h = _worldSetup.GridHeight;

        Vector2I cell = GD.RandRange(0, 3) switch
        {
            0 => new Vector2I(GD.RandRange(0, w - 1), 0),
            1 => new Vector2I(w - 1, GD.RandRange(0, h - 1)),
            2 => new Vector2I(GD.RandRange(0, w - 1), h - 1),
            _ => new Vector2I(0, GD.RandRange(0, h - 1)),
        };

        return _worldSetup.GridToWorld(cell);
    }
}
