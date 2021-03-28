using UnityEngine;

[CreateAssetMenu(fileName = "StatusResistance", menuName = "GDS/Resistances/StatusResistance")]
public class StatusResistance : Resistance
{
    [SerializeField] private UnitStatus _preventedStatus;
    public UnitStatus PreventedStatus { get { return _preventedStatus; } }

    private void Awake()
    {
        type = ResistanceType.Status;
    }
}
