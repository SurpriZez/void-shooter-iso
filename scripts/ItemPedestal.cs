using Godot;
using System;

public partial class ItemPedestal : Area3D
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
        // Base platform
        var baseMesh = new MeshInstance3D();
        var baseBox = new BoxMesh();
        baseBox.Size = new Vector3(0.8f, 0.08f, 0.4f);
        baseMesh.Mesh = baseBox;
        var baseMat = new StandardMaterial3D();
        baseMat.AlbedoColor = new Color(0.45f, 0.4f, 0.35f);
        baseMesh.MaterialOverride = baseMat;
        baseMesh.Position = new Vector3(0, 0.04f, 0);
        AddChild(baseMesh);

        // Floating icon
        var icon = new MeshInstance3D();
        icon.Name = "Icon";
        var sphere = new SphereMesh();
        sphere.Radius = 0.18f;
        sphere.Height = 0.36f;
        icon.Mesh = sphere;
        var iconMat = new StandardMaterial3D();
        iconMat.AlbedoColor = Item?.IconColor ?? new Color(0.6f, 0.2f, 0.8f);
        iconMat.EmissionEnabled = true;
        iconMat.Emission = (Item?.IconColor ?? new Color(0.6f, 0.2f, 0.8f)) * 0.4f;
        icon.MaterialOverride = iconMat;
        icon.Position = new Vector3(0, 0.55f, 0);
        AddChild(icon);

        var tween = icon.CreateTween();
        tween.SetLoops();
        tween.TweenProperty(icon, "position:y", 0.7f, 0.8f)
             .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(icon, "position:y", 0.55f, 0.8f)
             .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

        BodyEntered += OnBodyEntered;

        // Info zone — larger radius, shows panel before pickup
        var infoZone = new Area3D();
        infoZone.CollisionLayer = 0;
        infoZone.CollisionMask  = 1;
        var infoShape = new CollisionShape3D();
        var infoSphere = new SphereShape3D();
        infoSphere.Radius = 3f;
        infoShape.Shape = infoSphere;
        infoZone.AddChild(infoShape);
        AddChild(infoZone);
        infoZone.BodyEntered += body => { if (body is Player) GetHud()?.ShowItemInfo(Item); };
        infoZone.BodyExited  += body => { if (body is Player) GetHud()?.HideItemInfo(); };
    }

    private Hud GetHud() => _hud ??= GetTree().GetFirstNodeInGroup("hud") as Hud;

    private void OnBodyEntered(Node3D body)
    {
        if (_pickedUp || body is not Player player) return;
        _pickedUp = true;
        GetHud()?.HideItemInfo();
        player.PickupItem(Item);
        _onPickup?.Invoke();
        QueueFree();
    }
}
