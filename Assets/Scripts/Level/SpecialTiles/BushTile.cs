using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BushTile", menuName = "GDS/Level/BushTile")]
public class BushTile : LevelTile
{
    private List<Vector3Int> burningBushes = new List<Vector3Int>();

    [SerializeField]
    private UnitStatus inflictedStatus;

    [SerializeField]
    private GameObject flamePrefab;

    public override void OnEnter(Unit unitEntered)
    {
        if (burningBushes.Contains(unitEntered.CellPosition))
        {
            unitEntered.InflictStatus(inflictedStatus);
        }
    }

    public override void Activate(Vector3Int cellPos)
    {
        if (!burningBushes.Contains(cellPos))
        {
            burningBushes.Add(cellPos);
            TilemapNavigator navigator = TilemapNavigator.Instance;
            Unit occupyingUnit = navigator.GetUnit(cellPos);
            GameObject flame = Instantiate(flamePrefab);
            flame.transform.position = navigator.CellToWorldPos(cellPos);

            if (occupyingUnit != null)
            {
                OnEnter(occupyingUnit);
            }
        }
    }
}
