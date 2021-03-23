using UnityEngine;

[CreateAssetMenu(fileName = "SwampTile", menuName = "GDS/Level/SwampTile")]
public class SwampTile : LevelTile
{
    [SerializeField]
    private UnitStatus inflictedStatus;

    public override void OnUnitEnter(Unit unitEntered)
    {
        unitEntered.InflictStatus(inflictedStatus);
    }
}
