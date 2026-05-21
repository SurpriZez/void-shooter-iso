using Godot;
using System.Collections.Generic;

public partial class Hud : CanvasLayer
{
    private Player _player;
    private HBoxContainer _heartRow;
    private readonly List<ColorRect> _hearts = new();
    private HBoxContainer _itemRow;
    private int _knownItemCount;
    private Panel _infoPanel;
    private Label _infoName;
    private Label _infoDesc;
    private Panel _statsPanel;
    private Label[] _statLabels;

    private static readonly Color HeartFull  = new(0.9f, 0.2f, 0.2f, 1f);
    private static readonly Color HeartEmpty = new(0.2f, 0.2f, 0.2f, 1f);

    public override void _Ready()
    {
        AddToGroup("hud");
        _player = GetTree().GetFirstNodeInGroup("player") as Player;

        _heartRow = GetNode<HBoxContainer>("HBoxContainer");
        foreach (var child in _heartRow.GetChildren())
            child.QueueFree();
        _heartRow.AddThemeConstantOverride("separation", 4);

        _itemRow = new HBoxContainer();
        _itemRow.Position = new Vector2(12, 50);
        _itemRow.AddThemeConstantOverride("separation", 4);
        AddChild(_itemRow);

        _infoPanel = new Panel();
        _infoPanel.Position = new Vector2(12, 80);
        _infoPanel.CustomMinimumSize = new Vector2(260, 80);
        _infoPanel.Visible = false;
        AddChild(_infoPanel);

        var vbox = new VBoxContainer();
        vbox.Position = new Vector2(8, 6);
        _infoPanel.AddChild(vbox);

        _infoName = new Label();
        _infoName.AddThemeFontSizeOverride("font_size", 14);
        vbox.AddChild(_infoName);

        _infoDesc = new Label();
        _infoDesc.AddThemeFontSizeOverride("font_size", 11);
        _infoDesc.AutowrapMode = TextServer.AutowrapMode.Word;
        _infoDesc.CustomMinimumSize = new Vector2(244, 0);
        vbox.AddChild(_infoDesc);

        _statsPanel = new Panel();
        _statsPanel.AnchorLeft   = 1f;
        _statsPanel.AnchorRight  = 1f;
        _statsPanel.AnchorTop    = 0f;
        _statsPanel.AnchorBottom = 1f;
        _statsPanel.OffsetLeft   = -200f;
        _statsPanel.OffsetRight  = 0f;
        _statsPanel.Visible = false;
        var bg = new StyleBoxFlat();
        bg.BgColor = new Color(0f, 0f, 0f, 0.75f);
        bg.SetContentMarginAll(10f);
        _statsPanel.AddThemeStyleboxOverride("panel", bg);
        AddChild(_statsPanel);

        var svbox = new VBoxContainer();
        svbox.AddThemeConstantOverride("separation", 4);
        _statsPanel.AddChild(svbox);

        var title = new Label();
        title.Text = "— STATS —";
        title.AddThemeFontSizeOverride("font_size", 13);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        svbox.AddChild(title);

        string[] names = { "Speed", "Atk Cooldown", "Melee DMG", "Proj DMG",
                           "Knockback", "Health", "Melee Range", "Melee Angle" };
        _statLabels = new Label[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            _statLabels[i] = new Label();
            _statLabels[i].AddThemeFontSizeOverride("font_size", 11);
            svbox.AddChild(_statLabels[i]);
        }
    }

    public void ShowItemInfo(ItemData item)
    {
        if (item == null) return;
        _infoName.Text = item.ItemName;
        _infoName.AddThemeColorOverride("font_color", item.IconColor);
        _infoDesc.Text = item.Description;
        _infoPanel.Visible = true;
    }

    public void HideItemInfo() => _infoPanel.Visible = false;

    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventKey k && k.Pressed && !k.Echo && k.PhysicalKeycode == Key.C)
            _statsPanel.Visible = !_statsPanel.Visible;
    }

    public override void _Process(double delta)
    {
        if (_player == null || !IsInstanceValid(_player)) return;

        while (_hearts.Count < _player.MaxHealth)
        {
            var heart = new ColorRect();
            heart.CustomMinimumSize = new Vector2(26, 26);
            heart.Color = HeartEmpty;
            _heartRow.AddChild(heart);
            _hearts.Add(heart);
        }

        for (int i = 0; i < _hearts.Count; i++)
            _hearts[i].Color = i < _player.Health ? HeartFull : HeartEmpty;

        if (_statsPanel.Visible)
        {
            _statLabels[0].Text = $"Speed:        {_player.Speed:F0}";
            _statLabels[1].Text = $"Atk Cooldown: {_player.AttackCooldown:F2}s";
            _statLabels[2].Text = $"Melee DMG:    {_player.MeleeDamage}";
            _statLabels[3].Text = $"Proj DMG:     {_player.ProjectileDamage}";
            _statLabels[4].Text = $"Knockback:    {_player.KnockbackForce:F0}";
            _statLabels[5].Text = $"Health:       {_player.Health}/{_player.MaxHealth}";
            _statLabels[6].Text = $"Melee Range:  {_player.MeleeRange:F0}";
            _statLabels[7].Text = $"Melee Angle:  {_player.MeleeAngle:F0}°";
        }

        while (_knownItemCount < _player.Items.Count)
        {
            var item = _player.Items[_knownItemCount];
            var icon = new ColorRect();
            icon.CustomMinimumSize = new Vector2(16, 16);
            icon.Color = item.IconColor;
            _itemRow.AddChild(icon);
            _knownItemCount++;
        }
    }
}
