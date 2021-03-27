using System.Collections.Generic;
using UnityEngine;

struct FlameMeta
{
    public FlameMeta(int ticks, GameObject flame)
    {
        ticksLeft = ticks;
        fxObject = flame;
    }

    public int ticksLeft;
    public GameObject fxObject;

    public bool TriggerTick()
    {
        ticksLeft -= 1;
        bool shouldDestroy = ticksLeft <= 0;
        return shouldDestroy;
    }
} 

[CreateAssetMenu(fileName = "BushTile", menuName = "GDS/Level/BushTile")]
public class BushTile : LevelTile
{
    private Dictionary<Vector3Int, FlameMeta> burningBushes = new Dictionary<Vector3Int, FlameMeta>();

    [SerializeField]
    private int flameDuration;

    [SerializeField]
    private UnitStatus inflictedStatus;

    [SerializeField]
    private GameObject flamePrefab;

    public override void OnTick()
    {
        List<Vector3Int> bushes = new List<Vector3Int>(burningBushes.Keys);
        foreach(Vector3Int cellPos in bushes)
        {
            FlameMeta flame = burningBushes[cellPos];
            burningBushes[cellPos] = new FlameMeta(flame.ticksLeft - 1, flame.fxObject);
            if (burningBushes[cellPos].ticksLeft <= 0)
            {
                Destroy(burningBushes[cellPos].fxObject);
                burningBushes.Remove(cellPos);
            }
        }
    }

    public override bool OnUnitEnter(Unit unitEntered)
    {
        if (burningBushes.ContainsKey(unitEntered.CellPosition)
            && burningBushes[unitEntered.CellPosition].ticksLeft > 0)
        {
            unitEntered.statusManager.InflictStatus(inflictedStatus);
        }

        return true;
    }

    public override void Activate(Vector3Int cellPos)
    {

        if (!burningBushes.ContainsKey(cellPos))
        {
            burningBushes.Add(cellPos, new FlameMeta(flameDuration, CreateFlame(cellPos)));

            Unit occupyingUnit = TilemapNavigator.Instance.GetUnit(cellPos);
            if (occupyingUnit != null) OnUnitEnter(occupyingUnit);
        } else
        {
            ResetTicks(cellPos);
        }
    }

    private GameObject CreateFlame(Vector3Int cellPos)
    {
        GameObject flame = Instantiate(flamePrefab);
        flame.transform.position = TilemapNavigator.Instance.CellToWorldPos(cellPos);
        return flame;
    }

    private void ResetTicks(Vector3Int cellPos)
    {
        FlameMeta flame = burningBushes[cellPos];
        burningBushes[cellPos] = new FlameMeta(flameDuration, flame.fxObject);
    }
}
