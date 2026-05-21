using Godot;
using System;

public partial class ItemPedestal : Area2D
{
    public ItemData Item { get; private set; }
    private Action _onPickup;
    private bool _pickedUp;
    private Hud _hud;

    public void Initialize(ItemData item, Action onPickup)
    {
        Item = item;
        _onPickup = onPickup;
    }

    public override void _Ready()
    {
        var baseNode = GetNode<Polygon2D>("Base");
        baseNode.Polygon = new Vector2[] { new(-20,-4), new(20,-4), new(20,4), new(-20,4) };
        baseNode.Color = new Color(0.45f, 0.4f, 0.35f);

        var icon = GetNode<Polygon2D>("Icon");
        icon.Polygon = new Vector2[] { new(0,-14), new(10,6), new(-10,6) };
        icon.Color = Item?.IconColor ?? new Color(0.6f, 0.2f, 0.8f);

        var tween = icon.CreateTween();
        tween.SetLoops();
        tween.TweenProperty(icon, "position:y", icon.Position.Y - 6f, 0.8f)
             .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(icon, "position:y", icon.Position.Y, 0.8f)
             .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

        BodyEntered += OnBodyEntered;

        // Larger info zone — shows item panel before player walks into pickup radius
        var infoZone = new Area2D();
        infoZone.CollisionLayer = 0;
        infoZone.CollisionMask = 1;
        var infoShape = new CollisionShape2D();
        var circle = new CircleShape2D();
        circle.Radius = 65f;
        infoShape.Shape = circle;
        infoZone.AddChild(infoShape);
        AddChild(infoZone);
        infoZone.BodyEntered += body => { if (body is Player) GetHud()?.ShowItemInfo(Item); };
        infoZone.BodyExited  += body => { if (body is Player) GetHud()?.HideItemInfo(); };
    }

    private Hud GetHud() => _hud ??= GetTree().GetFirstNodeInGroup("hud") as Hud;

    private void OnBodyEntered(Node2D body)
    {
        if (_pickedUp || body is not Player player) return;
        _pickedUp = true;
        GetHud()?.HideItemInfo();
        player.PickupItem(Item);
        _onPickup?.Invoke();
        QueueFree();
    }
}
