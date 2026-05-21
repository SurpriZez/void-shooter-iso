using Godot;

public partial class EnemySpawner : Node2D
{
    [Export] public PackedScene EnemyScene;
    [Export] public int EnemyCount = 3;
    [Export] public float SpawnRadius = 220f;
    [Export] public float WaveDelay = 2f;

    private enum State { Active, Cooldown }
    private State _state = State.Active;
    private float _timer;

    public override void _Ready() => SpawnWave();

    public override void _Process(double delta)
    {
        if (_state == State.Cooldown)
        {
            _timer -= (float)delta;
            if (_timer <= 0)
            {
                _state = State.Active;
                SpawnWave();
            }
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
        float angleOffset = GD.Randf() * Mathf.Tau;
        for (int i = 0; i < EnemyCount; i++)
        {
            var enemy = EnemyScene.Instantiate<Enemy>();
            float angle = angleOffset + (float)i / EnemyCount * Mathf.Tau;
            enemy.GlobalPosition = GlobalPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * SpawnRadius;
            enemy.ZIndex = 1;
            GetParent().AddChild(enemy);
        }
    }
}
