using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

public class UnitBlockManager : MonoBehaviour
{
    private static UnitBlockManager _instance;
    public static UnitBlockManager Instance {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UnitBlockManager>();
            }

            return _instance;
        }
    }

    public TilesetTraversalProvider traversalProvider;
    public List<SingleNodeBlocker> obstacles;

    void Awake()
    {
        traversalProvider = new TilesetTraversalProvider();
    }

    private void Start()
    {
        BlockManager blockManager = GetComponent<BlockManager>();
        List<SingleNodeBlocker> obstacles = FindObjectsOfType<SingleNodeBlocker>().ToList();

        foreach (SingleNodeBlocker obstacle in obstacles)
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