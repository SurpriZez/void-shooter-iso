using Godot;

[GlobalClass]
public abstract partial class ItemEffect : Resource
{
    public virtual void OnPickup(Player player) { }
    public virtual void OnProjectileSpawn(Projectile projectile) { }
    public virtual void OnProjectileHit(Enemy enemy, Projectile projectile) { }
    public virtual void OnMeleeHit(Enemy enemy, Player player) { }
    public virtual void OnPlayerProcess(Player player, double delta) { }
}
