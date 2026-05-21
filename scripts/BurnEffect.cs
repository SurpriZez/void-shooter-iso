using Godot;

[GlobalClass]
public partial class BurnEffect : ItemEffect
{
    [Export] public float Duration = 3f;
    [Export] public float DamagePerSecond = 8f;

    public override void OnProjectileHit(Enemy enemy, Projectile _)
        => enemy.ApplyStatus(new BurnStatus(Duration, DamagePerSecond));

    public override void OnMeleeHit(Enemy enemy, Player _)
        => enemy.ApplyStatus(new BurnStatus(Duration, DamagePerSecond));
}

public class BurnStatus : StatusEffect
{
    private float _dps;
    private float _accumulator;

    public override string Id => "burn";

    public float Dps => _dps;

    public BurnStatus(float duration, float dps) : base(duration) => _dps = dps;

    public override void OnStack(StatusEffect incoming)
    {
        if (incoming is BurnStatus burn)
            _dps += burn._dps;
        Refresh(incoming.TimeRemaining);
    }

    public override void Tick(Enemy enemy, float delta)
    {
        _accumulator += _dps * delta;
        if (_accumulator >= 1f)
        {
            int dmg = (int)_accumulator;
            _accumulator -= dmg;
            enemy.TakeDotDamage(dmg);
        }
    }
}
