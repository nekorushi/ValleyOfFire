using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

public class UnitBlockManager : MonoBehaviour
{
    public static UnitBlockManager Instance;

    public TilesetTraversalProvider traversalProvider;
    public List<SingleNodeBlocker> obstacles;

    void Start()
    {
        Instance = this;

        BlockManager blockManager = GetComponent<BlockManager>();
        List<SingleNodeBlocker> obstacles = FindObjectsOfType<SingleNodeBlocker>().ToList();

        traversalProvider = new TilesetTraversalProvider();

        foreach(SingleNodeBlocker obstacle in obstacles)
        {
            obstacle.manager = blockManager;
        }
    }
}

public class TilesetTraversalProvider : ITraversalProvider
{
    public Vector3Int selectedUnitCell;
    List<Vector3Int> takenNodes = new List<Vector3Int>();

    public bool CanTraverse(Path path, GraphNode node)
    {
        Vector3 floatPos = (Vector3)node.position;
        Vector3Int nodeCellPos = new Vector3Int((int)floatPos.x, (int)floatPos.y, (int)floatPos.z);
        bool canTraverse = !takenNodes
            .Where(cellPos => cellPos != selectedUnitCell)
            .Contains(nodeCellPos);
        return DefaultITraversalProvider.CanTraverse(path, node) && canTraverse;
    }

    public uint GetTraversalCost(Path path, GraphNode node)
    {
        return DefaultITraversalProvider.GetTraversalCost(path, node);
    }

    public void ReserveNode(Vector3Int position)
    {
        if (!takenNodes.Contains(position)) takenNodes.Add(position);
    }

    public void ReleaseNode(Vector3Int position)
    {
        if (takenNodes.Contains(position)) takenNodes.Remove(position);
    }
}