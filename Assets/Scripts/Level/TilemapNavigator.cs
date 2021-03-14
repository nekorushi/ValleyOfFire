using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;

class TilemapNavigator : MonoBehaviour
{
    private static TilemapNavigator _instance;
    public static TilemapNavigator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TilemapNavigator>();
            }

            return _instance;
        }
    }

    public static Vector3 CellCenterOffset = new Vector3(.5f, .5f, 0);
    TilesetGraph pathfindingGraph;

    [SerializeField]
    private LayerMask clickableLayerMask;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Tilemap tilemap;

    private void Start()
    {
        grid = GetComponent<Grid>();
        pathfindingGraph = (TilesetGraph)AstarPath.active.data.FindGraph(g => g.name == "TilesetGraph");
    }

    public bool HasTile(Vector3Int position)
    {
        return tilemap.HasTile(position);
    }

    public LevelTile GetTile(Vector3Int position)
    {
        return tilemap.GetTile<LevelTile>(position);
    }

    public Unit GetUnit(Vector3Int cellPos)
    {
        Vector3 fieldWorldPos = CellToWorldPos(cellPos);
        return GetUnit(fieldWorldPos);
    }

    public Unit GetUnit(Vector3 worldPos)
    {
        GameObject foundObject = GetObjectAtWorldPos(worldPos);
        return foundObject != null ? foundObject.GetComponent<Unit>() : null;
    }

    public BoundsInt GetTilemapBounds()
    {
        return tilemap.cellBounds;
    }

    public GameObject GetObjectAtWorldPos(Vector3 worldPos)
    {
        Vector3 raycastPos = new Vector3(worldPos.x, worldPos.y, -10);
        RaycastHit2D hit = Physics2D.Raycast(raycastPos, Vector2.zero, Mathf.Infinity, clickableLayerMask);

        DrawTarget(new Vector3(hit.point.x, hit.point.y, 0), Color.blue);

        if (hit.collider != null)
        {
            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

    public Vector3 CellToWorldPos(Vector3Int position)
    {
        return grid.CellToWorld(position) + CellCenterOffset;
    }

    public Vector3Int WorldToCellPos(Vector3 worldPos)
    {
        return grid.WorldToCell(worldPos);
    }

    public Vector3Int Int3ToCellPos(Int3 position)
    {
        Vector3 floatPos = (Vector3)position;
        return new Vector3Int((int)floatPos.x, (int)floatPos.y, (int)floatPos.z);
    }

    public bool IsTileTaken(Vector3Int position)
    {
        Vector3 worldPos = CellToWorldPos(position);
        GameObject hitObject = GetObjectAtWorldPos(worldPos);

        bool unitFound = hitObject != null && hitObject.GetComponent<Unit>() != null;

        return unitFound;
    }

    public bool IsTileMoveable(Vector3Int position)
    {
        LevelTile tile = GetTile(position);
        return tile != null && tile.Type == TileType.Walkable;
    }

    private void DrawTarget(Vector3 worldPos, Color color)
    {
        float radius = 0.3f;

        Vector3 firstStart = new Vector3(worldPos.x - radius, worldPos.y - radius, -5f);
        Vector3 firstEnd = new Vector3(worldPos.x + radius, worldPos.y + radius, -5f);
        Vector3 secondStart = new Vector3(worldPos.x - radius, worldPos.y + radius, -5f);
        Vector3 secondEnd = new Vector3(worldPos.x + radius, worldPos.y - radius, -5f);

        Debug.DrawLine(firstStart, firstEnd, color, 2f);
        Debug.DrawLine(secondStart, secondEnd, color, 2f);
    }

    public List<Vector3Int> CalculateMovementRange(Vector3Int startingCell, int range)
    {
        Int3 startingNodePos = (Int3)(Vector3)startingCell;
        PointNode initialNode = Array.Find(pathfindingGraph.nodes, node => node.position == startingNodePos);
        List<GraphNode> reachableNodes = PathUtilities.BFS(initialNode, range, -1, node => {
            Vector3Int nodeCellPos = Int3ToCellPos(node.position);
            bool result = nodeCellPos == startingCell || !IsTileTaken(nodeCellPos);
            return result;
        });

        List<Vector3Int> moves = new List<Vector3Int>();
        foreach(GraphNode node in reachableNodes)
        {
            if (node.position != startingNodePos) moves.Add(Int3ToCellPos(node.position));
        }
        return moves;
    }
}
