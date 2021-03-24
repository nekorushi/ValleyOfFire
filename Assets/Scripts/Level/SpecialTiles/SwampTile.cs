using UnityEngine;

[CreateAssetMenu(fileName = "SwampTile", menuName = "GDS/Level/SwampTile")]
public class SwampTile : LevelTile
{
    [SerializeField]
    private UnitStatus inflictedStatus;

    public override bool OnUnitEnter(Unit unitEntered)
    {
        unitEntered.InflictStatus(inflictedStatus);
        return false;
    }

    public override void OnUnitLeave(Unit unitEntered)
    {
        unitEntered.RemoveStatus();
    }
}
