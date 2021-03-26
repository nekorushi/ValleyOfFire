using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    Walkable,
    NonWalkable,
}

[CreateAssetMenu(fileName = "LevelTile", menuName = "GDS/Level/Tile")]
public class LevelTile : Tile
{
    [Header("Custom fields")]
    [SerializeField]
    private TileType _type;
    public TileType Type
    {
        get { return _type; }
    }

    [SerializeField]
    private uint _cost;
    public uint Cost
    {
        get { return _cost; }
    }

    [SerializeField]
    private bool _canBeAttacked = false;
    public bool CanBeAttacked
    {
        get { return _canBeAttacked; }
    }

    private void OnEnable()
    {
        flags = TileFlags.LockTransform;
    }

    public virtual bool OnUnitEnter(Unit unitEntered) { return true; }
    public virtual void OnUnitLeave(Unit unitEntered) { }
    public virtual void OnTick() { }
    public virtual void Activate(Vector3Int cellPos) { }
}
