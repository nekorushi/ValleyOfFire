using System.Collections.Generic;

public class ActionPoints
{
    public ActionPoints(bool _didMove, bool _didAttack)
    {
        canMove = _didMove;
        canAttack = _didAttack;
    }

    public bool canMove;
    public bool canAttack;
}

public class PlayerTurnManager
{
    private int _maxUnitsPerTurn = 2;
    public int MaxUnitsPerTurn { get { return _maxUnitsPerTurn; } }

    private bool _turnFinished = false;
    public bool TurnFinished
    {
        get { return _turnFinished; }
    }

    private Dictionary<Unit, ActionPoints> _activeUnits = new Dictionary<Unit, ActionPoints>();
    public Dictionary<Unit, ActionPoints> ActiveUnits { get { return _activeUnits; } }

    private void ActivateUnit(Unit unit)
    {
        if (!IsActive(unit) && _activeUnits.Count < _maxUnitsPerTurn)
        {
            _activeUnits.Add(unit, new ActionPoints(true, true));
        }
    }

    public bool CanPerformMovement(Unit unit)
    {
        if (IsActive(unit))
        {
            return _activeUnits[unit].canMove;
        } else {
            return CanBeSelected(unit);
        }
    }

    public bool CanPerformAction(Unit unit)
    {
        if (IsActive(unit))
        {
            return _activeUnits[unit].canAttack;
        }
        else
        {
            return CanBeSelected(unit);
        }
    }

    public bool CanBeSelected(Unit unit)
    {
        return IsActive(unit) || _activeUnits.Count < _maxUnitsPerTurn;
    }

    public bool IsActive(Unit unit)
    {
        return _activeUnits.ContainsKey(unit);
    }

    public bool UseMovementPoint(Unit unit)
    {
        ActivateUnit(unit);
        if (CanPerformMovement(unit))
        {
            _activeUnits[unit].canMove = false;
            return true;
        }

        return false;
    }

    public bool UseActionPoint(Unit unit)
    {
        ActivateUnit(unit);
        if (CanPerformAction(unit))
        {
            _activeUnits[unit].canAttack = false;
            _activeUnits[unit].canMove = false;
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
