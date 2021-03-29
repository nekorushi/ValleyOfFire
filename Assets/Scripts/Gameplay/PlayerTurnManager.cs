using System.Collections.Generic;
using UnityEngine.Events;

public class PlayerTurnManager
{
    private int _maxUnitsPerTurn = 2;
    public int MaxUnitsPerTurn { get { return _maxUnitsPerTurn; } }
    private int _actionPointsPerUnit = 2;

    private bool _turnFinished = false;
    public bool TurnFinished
    {
        get { return _turnFinished; }
    }

    private Dictionary<Unit, int> _activeUnits = new Dictionary<Unit, int>();
    public Dictionary<Unit, int> ActiveUnits { get { return _activeUnits; } }

    private void ActivateUnit(Unit unit)
    {
        if (!_activeUnits.ContainsKey(unit) && _activeUnits.Count < _maxUnitsPerTurn)
        {
            _activeUnits.Add(unit, _actionPointsPerUnit);
        }
    }

    public bool CanPerformAction(Unit unit)
    {
        return _activeUnits[unit] > 0;
    }

    public bool CanBeSelected(Unit unit)
    {
        return _activeUnits.ContainsKey(unit) || _activeUnits.Count < _maxUnitsPerTurn;
    }

    public bool UseActionPoint(Unit unit, bool useAll = false)
    {
        ActivateUnit(unit);
        if (CanPerformAction(unit))
        {
            if (useAll)
            {
                _activeUnits[unit] = 0;
            } else
            {
                _activeUnits[unit]--;
            }

            return true;
        }

        return false;
    }

    public void FinishTurn()
    {
        _turnFinished = true;
    }

    public void ResetTurn()
    {
        _activeUnits.Clear();
        _turnFinished = false;
    }
}
