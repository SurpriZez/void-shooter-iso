using Godot;

[GlobalClass]
public partial class ExplosionEffect : ItemEffect
{
    [Export] public float Radius = 55f;
    [Export] public int SplashDamage = 18;

    public override void OnProjectileHit(Enemy primaryEnemy, Projectile projectile)
    {
        SpawnFlash(projectile);

        var query = new PhysicsShapeQueryParameters2D();
        query.Shape = new CircleShape2D { Radius = Radius };
        query.Transform = new Transform2D(0, projectile.GlobalPosition);
        query.CollisionMask = 4;

        foreach (var hit in projectile.GetWorld2D().DirectSpaceState.IntersectShape(query))
            if (hit["collider"].AsGodotObject() is Enemy enemy)
                enemy.TakeDamage(SplashDamage);
    }

    private static void SpawnFlash(Node2D anchor)
    {
        var circle = new Polygon2D();
        circle.Polygon = MakeCircle(55f, 16);
        circle.Color = new Color(1f, 0.6f, 0.1f, 0.55f);
        circle.GlobalPosition = anchor.GlobalPosition;
        circle.ZIndex = 5;
        anchor.GetParent().AddChild(circle);

        var tween = circle.CreateTween();
        tween.TweenProperty(circle, "modulate:a", 0f, 0.25f);
        tween.TweenCallback(Callable.From(circle.QueueFree));
    }

    private static Vector2[] MakeCircle(float r, int segments)
    {
        var pts = new Vector2[segments];
        for (int i = 0; i < segments; i++)
        {
            float a = i / (float)segments * Mathf.Tau;
            pts[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
        }
        return pts;
    }
}
