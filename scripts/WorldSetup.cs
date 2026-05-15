using Godot;

public partial class WorldSetup : TileMapLayer
{
    public override void _Ready()
    {
        var image = Image.CreateEmpty(64, 32, false, Image.Format.Rgba8);
        image.Fill(new Color(0.35f, 0.55f, 0.75f));
        var texture = ImageTexture.CreateFromImage(image);

        var source = new TileSetAtlasSource();
        source.Texture = texture;
        source.TextureRegionSize = new Vector2I(64, 32);
        source.CreateTile(new Vector2I(0, 0));

        var tileSet = new TileSet();
        tileSet.TileShape = TileSet.TileShapeEnum.Isometric;
        tileSet.TileSize = new Vector2I(64, 32);
        int sourceId = tileSet.AddSource(source);
        TileSet = tileSet;

        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                SetCell(new Vector2I(x, y), sourceId, new Vector2I(0, 0));
    }
}
