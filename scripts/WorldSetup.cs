using Godot;

public partial class WorldSetup : TileMapLayer
{
    [Export] public int GridWidth = 10;
    [Export] public int GridHeight = 10;
    [Export] public Color TileColor = new Color(0.35f, 0.55f, 0.75f);

    public override void _Ready()
    {
        var image = Image.CreateEmpty(64, 32, false, Image.Format.Rgba8);
        var borderColor = TileColor.Darkened(0.35f);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                // distance from diamond edge (0 = edge, 1 = center, <0 = outside)
                float edgeDist = 1.0f - (Mathf.Abs(x - 32f) / 32f + Mathf.Abs(y - 16f) / 16f);
                if (edgeDist < 0f) continue;
                image.SetPixel(x, y, edgeDist < 0.1f ? borderColor : TileColor);
            }
        }
        var texture = ImageTexture.CreateFromImage(image);

        var source = new TileSetAtlasSource();
        source.Texture = texture;
        source.TextureRegionSize = new Vector2I(64, 32);
        source.CreateTile(new Vector2I(0, 0));

        var tileSet = new TileSet();
        tileSet.TileShape = TileSet.TileShapeEnum.Isometric;
        tileSet.TileSize = new Vector2I(64, 32);
        int sourceId = tileSet.AddSource(source);
        TileSet = tileSet; // must assign before SetCell so source IDs are valid

        for (int x = 0; x < GridWidth; x++)
            for (int y = 0; y < GridHeight; y++)
                SetCell(new Vector2I(x, y), sourceId, new Vector2I(0, 0));
    }
}
