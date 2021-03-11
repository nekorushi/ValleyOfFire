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

}
