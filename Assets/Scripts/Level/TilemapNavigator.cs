using UnityEngine;
using UnityEngine.Tilemaps;

class TilemapNavigator : MonoBehaviour
{
    public static TilemapNavigator Instance;

    [SerializeField]
    private LayerMask clickableLayerMask;

    public TilemapNavigator()
    {
        Instance = this;
    }

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Tilemap tilemap;

    private void Start()
    {
        grid = GetComponent<Grid>();
    }

    public bool HasTile(Vector3Int position)
    {
        return tilemap.HasTile(position);
    }

    public LevelTile GetTile(Vector3Int position)
    {
        return tilemap.GetTile<LevelTile>(position);
    }

    public Vector3 CellToWorldPos(Vector3Int position)
    {
        return grid.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0);
    }

    public Vector3Int WorldToCellPos(Vector3 worldPos)
    {
        return grid.WorldToCell(worldPos);
    }

    public bool IsTileTaken(Vector3Int position)
    {
        Vector3 worldPos = CellToWorldPos(position);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, clickableLayerMask);

        if (hit.collider != null)
        {
            Unit unitHit = hit.collider.gameObject.GetComponent<Unit>();
            return (bool)unitHit;
        } else
        {
            return false;
        }

    }

    public bool IsTileMoveable(Vector3Int position)
    {
        LevelTile tile = GetTile(position);
        return tile != null && tile.Type == TileType.Walkable;
    }
}