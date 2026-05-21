using Godot;

public partial class WorldSetup : Node3D
{
    [Export] public int GridWidth  = 10;
    [Export] public int GridHeight = 10;
    [Export] public float TileSize = 2f;
    [Export] public Color TileColor = new Color(0.35f, 0.55f, 0.75f);

    public override void _Ready()
    {
        // Floor collision — single static body covering the whole grid
        var floor = new StaticBody3D();
        floor.CollisionLayer = 16;
        floor.CollisionMask  = 0;
        var floorShape = new CollisionShape3D();
        var box = new BoxShape3D();
        box.Size = new Vector3(GridWidth * TileSize, 0.2f, GridHeight * TileSize);
        floorShape.Shape = box;
        floor.AddChild(floorShape);
        floor.Position = new Vector3(0, -0.1f, 0);
        AddChild(floor);

        // Visual tiles — shared mesh, unique positions
        var tileMesh = new BoxMesh();
        tileMesh.Size = new Vector3(TileSize - 0.05f, 0.1f, TileSize - 0.05f);

        var mat = new StandardMaterial3D();
        mat.AlbedoColor = TileColor;
        mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;

        var borderMat = new StandardMaterial3D();
        borderMat.AlbedoColor = TileColor.Darkened(0.3f);
        borderMat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;

        for (int x = 0; x < GridWidth; x++)
            for (int z = 0; z < GridHeight; z++)
            {
                var tile = new MeshInstance3D();
                tile.Mesh = tileMesh;
                tile.MaterialOverride = (x + z) % 2 == 0 ? mat : borderMat;
                tile.Position = GridToWorld(new Vector2I(x, z));
                AddChild(tile);
            }
    }

    public Vector3 GridToWorld(Vector2I cell)
    {
        float x = (cell.X - GridWidth  / 2.0f + 0.5f) * TileSize;
        float z = (cell.Y - GridHeight / 2.0f + 0.5f) * TileSize;
        return new Vector3(x, 0, z);
    }
}
