using Godot;

[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string ItemName = "";
    [Export] public Color IconColor = new Color(0.6f, 0.2f, 0.8f);
    [Export] public float SpeedBonus = 0f;
    [Export] public float AttackCooldownBonus = 0f;
    [Export] public int MeleeDamageBonus = 0;
    [Export] public int ProjectileDamageBonus = 0;
    [Export] public int MaxHealthBonus = 0;
    [Export] public float KnockbackBonus = 0f;
}
