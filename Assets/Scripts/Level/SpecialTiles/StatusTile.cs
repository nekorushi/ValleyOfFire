using UnityEngine;

[CreateAssetMenu(fileName = "StatusTile", menuName = "GDS/Level/StatusTile")]
public class StatusTile : LevelTile
{
    [SerializeField]
    private UnitStatus inflictedStatus;

    public override void OnEnter(Unit unitEntered)
    {
        unitEntered.InflictStatus(inflictedStatus);
    }
}
