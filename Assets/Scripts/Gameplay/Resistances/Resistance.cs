using System.Collections.Generic;
using UnityEngine;

public enum ImmunityTypes
{
    Status,
    Trajectory,
}

[CreateAssetMenu(fileName = "Resistance", menuName = "GDS/Resistance", order = 1)]
public class Resistance : ScriptableObject
{
    private Dictionary<Unit, int> activeResistances = new Dictionary<Unit, int>();

    [SerializeField] private bool _isPermanent;
    [SerializeField] private int _duration;
    [SerializeField] private UnitStatus _preventedStatus;


    public UnitStatus PreventedStatus { get { return _preventedStatus; } }

    public void OnAdd(Unit immuneUnit)
    {
        if (!_isPermanent)
        {
            if (activeResistances.ContainsKey(immuneUnit))
            {
                activeResistances[immuneUnit] = _duration;
            } else
            {
                activeResistances.Add(immuneUnit, _duration);
            }
        }
    }

    public bool OnTick(Unit immuneUnit)
    {
        if (!_isPermanent)
        {
            if (activeResistances.ContainsKey(immuneUnit))
            {
                activeResistances[immuneUnit]--;
                return activeResistances[immuneUnit] <= 0;
            } else
            {
                return true;
            }
        }

        return false;
    }
}
