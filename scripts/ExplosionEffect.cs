using Godot;

[GlobalClass]
public partial class ExplosionEffect : ItemEffect
{
    [Export] public float Radius = 3f;
    [Export] public int SplashDamage = 18;

    public override void OnProjectileHit(Enemy primaryEnemy, Projectile projectile)
    {
        SpawnFlash(projectile);

        var query = new PhysicsShapeQueryParameters3D();
        query.Shape = new SphereShape3D { Radius = Radius };
        query.Transform = new Transform3D(Basis.Identity, projectile.GlobalPosition);
        query.CollisionMask = 4;

        foreach (var hit in projectile.GetWorld3D().DirectSpaceState.IntersectShape(query))
            if (hit["collider"].AsGodotObject() is Enemy enemy)
                enemy.TakeDamage(SplashDamage);
    }

    private static void SpawnFlash(Node3D anchor)
    {
        var flash = new MeshInstance3D();
        var cyl = new CylinderMesh();
        cyl.TopRadius    = 3f;
        cyl.BottomRadius = 3f;
        cyl.Height       = 0.05f;
        flash.Mesh = cyl;

        var mat = new StandardMaterial3D();
        mat.AlbedoColor = new Color(1f, 0.6f, 0.1f, 0.55f);
        mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        mat.ShadingMode  = BaseMaterial3D.ShadingModeEnum.Unshaded;
        flash.MaterialOverride = mat;
        flash.GlobalPosition = anchor.GlobalPosition + new Vector3(0, 0.03f, 0);
        anchor.GetParent().AddChild(flash);

        var tween = flash.CreateTween();
        tween.TweenProperty(mat, "albedo_color:a", 0f, 0.25f);
        tween.TweenCallback(Callable.From(flash.QueueFree));
    }
}
