using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.Serialization;
using Pathfinding.Util;

[JsonOptIn]
[Pathfinding.Util.Preserve]
public class TilesetGraph : NavGraph
{
    public PointNode[] nodes;

    protected override IEnumerable<Progress> ScanInternal()
    {
        TilemapNavigator navigator = TilemapNavigator.Instance;
        BoundsInt mapBounds = navigator.GetTilemapBounds();
        Dictionary<int, Dictionary<int, PointNode>> gridNodes = new Dictionary<int, Dictionary<int, PointNode>>();

        // Find all tiles
        for (int xPos = mapBounds.xMin; xPos <= mapBounds.xMax; xPos++)
        {
            for (int yPos = mapBounds.yMin; yPos <= mapBounds.yMax; yPos++)
            {
                Vector3Int cellPos = GetCellPos(xPos, yPos);
                if (navigator.HasTile(cellPos)) {
                    if (!gridNodes.ContainsKey(xPos)) gridNodes.Add(xPos, new Dictionary<int, PointNode>());

                    gridNodes[xPos].Add(yPos, CreateNode(cellPos));
                }
            }
        }

        // Connect adjacent tiles
        for (int xPos = mapBounds.xMin; xPos <= mapBounds.xMax; xPos++)
        {
            for (int yPos = mapBounds.yMin; yPos <= mapBounds.yMax; yPos++)
            {
                PointNode node = GetNodeInDict(gridNodes, xPos, yPos);
                if (node != null && node.Walkable)
                {
                    Vector3Int cellPos = GetCellPos(xPos, yPos);
                    LevelTile tile = navigator.GetTile(cellPos);
                    List<Connection> connections = new List<Connection>();
                    Vector2Int[] neighbors =
                    {
                        Vector2Int.up,
                        Vector2Int.down,
                        Vector2Int.left,
                        Vector2Int.right,
                    };

                    node.Penalty = tile.Cost;

                    foreach (Vector2Int offset in neighbors)
                    {
                        PointNode neighborNode = GetNodeInDict(gridNodes, xPos + offset.x, yPos + offset.y);
                        if (neighborNode != null && neighborNode.Walkable)
                        {
                            connections.Add(new Connection(neighborNode, 0));
                        }
                    }

                    node.connections = connections.ToArray();
                }
            }
        }


        List<PointNode> allNodes = new List<PointNode>();
        foreach(KeyValuePair<int, Dictionary<int, PointNode>> column in gridNodes)
        {
            foreach(KeyValuePair<int, PointNode> cell in column.Value)
            {
                allNodes.Add(cell.Value);
            }
        }
        nodes = allNodes.ToArray();
        yield break;
    }
    
    private PointNode GetNodeInDict(Dictionary<int, Dictionary<int, PointNode>> gridNodes, int xPos, int yPos)
    {
        if (gridNodes.ContainsKey(xPos))
        {
            if (gridNodes[xPos].ContainsKey(yPos))
                return gridNodes[xPos][yPos];
        }

        return null;
    }

    private Vector3Int GetCellPos(int xPos, int yPos)
    {
        return new Vector3Int(xPos, yPos, 0);
    }

    private PointNode CreateNode(Vector3Int cellPos)
    {
        TilemapNavigator navigator = TilemapNavigator.Instance;
        LevelTile tile = navigator.GetTile(cellPos);
        PointNode node = new PointNode(active);
        Vector3 floatCellPos = cellPos;
        node.position = (Int3)floatCellPos;
        node.Walkable = tile.Type == TileType.Walkable;
        return node;
    }

    public override void GetNodes(System.Action<GraphNode> action)
    {
        if (nodes == null) return;

        for (int i = 0; i < nodes.Length; i++)
        {
            // Call the delegate
            action(nodes[i]);
        }
    }
}