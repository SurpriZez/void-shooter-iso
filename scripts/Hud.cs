using Godot;

public partial class Hud : CanvasLayer
{
    private Player _player;
    private ColorRect[] _hearts;

    private static readonly Color HeartFull  = new(0.9f, 0.2f, 0.2f, 1f);
    private static readonly Color HeartEmpty = new(0.2f, 0.2f, 0.2f, 1f);

    public override void _Ready()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        _hearts = new[]
        {
            GetNode<ColorRect>("HBoxContainer/Heart1"),
            GetNode<ColorRect>("HBoxContainer/Heart2"),
            GetNode<ColorRect>("HBoxContainer/Heart3"),
        };
    }

    public override void _Process(double delta)
    {
        if (_player == null || !IsInstanceValid(_player)) return;
        for (int i = 0; i < _hearts.Length; i++)
            _hearts[i].Color = i < _player.Health ? HeartFull : HeartEmpty;
    }
}
