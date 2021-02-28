using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ControlModes
{
    Movement,
    Attack
}

public class PlayerController : MonoBehaviour
{
    public UnityEvent UnitSelectionChanged;
    public UnityEvent ControlModeChanged;
    public UnityEvent AvailableActionsChanged;

    [Header("Component configuration")]
    [SerializeField]
    private LayerMask layerMask;
    private Camera mainCamera;
    private byte currentActionPoints = 0;

    [Header("Gameplay settings")]
    [SerializeField]
    private string _playerName;
    public string PlayerName { get { return _playerName; } }

    [SerializeField]
    private Color _playerColor;
    public Color PlayerColor { get { return _playerColor; } }

    private ControlModes _controlMode = ControlModes.Movement;
    public ControlModes ControlMode {
        get { return _controlMode; }
        private set
        {
            _controlMode = value;
            ControlModeChanged.Invoke();
        }
    }

    [SerializeField]
    private byte maxActionPoints = 2;

    [SerializeField]
    private List<Unit> _units;
    public List<Unit> Units {
        get { return _units; }
        private set { _units = value; }
    }

    private Unit _currentUnit;  
    public Unit CurrentUnit {
        get { return _currentUnit; }
        private set
        {
            _currentUnit?.Blur();
            _currentUnit = value;
            _currentUnit?.Focus();
            UnitSelectionChanged.Invoke();
        } 
    }

    void Start()
    {
        mainCamera = Camera.main;
        AssignOwnedUnits();
    }

    private void Reset()
    {
        SelectUnit(null);
    }

    public bool HasAliveUnits {get {return Units.Where(unit => unit.Health > 0).ToList().Count > 0; } }

    private void AssignOwnedUnits()
    {
        Units.ForEach(unit =>
        {
            if (unit != null)
            {
                unit.SetOwner(this);
            }
        });
    }

    public void ChangeControlMode()
    {
        ControlMode = ControlMode == ControlModes.Attack ? ControlModes.Movement : ControlModes.Attack;
    }

    public IEnumerator PerformTurn()
    {
        currentActionPoints = maxActionPoints;

        while(currentActionPoints > 0)
        {
            if (CurrentUnit && CurrentUnit.AttackPattern.attackType == AttackType.Area)
            {
                UpdateAttackArea();
            }

            if (Input.GetMouseButtonDown(0))
            {
                yield return StartCoroutine(HandleMouseClick());
            }

            yield return null;
        }
        Reset();
    }

    private IEnumerator HandleMouseClick()
    {
        Vector3 clickedWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        GameObject clickedObject = TilemapNavigator.Instance.GetObjectAtWorldPos(clickedWorldPos);

        if (clickedObject != null)
        {
            Unit clickedUnit = clickedObject.GetComponent<Unit>();
            bool clickedOwnUnit = Units.Contains(clickedUnit);

            if (clickedOwnUnit)
            {
                bool shouldUnselect = CurrentUnit == clickedUnit;
                SelectUnit(shouldUnselect ? null : clickedUnit);
            } else
            {
                Vector3Int clickedCellPos = TilemapNavigator.Instance.WorldToCellPos(clickedWorldPos);
                if (CurrentUnit) yield return StartCoroutine(PerformUnitAction(clickedCellPos, clickedUnit));
            }
        }
    }

    private void UpdateAttackArea()
    {
        Vector3 cursorWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 unitWorldPos = CurrentUnit.transform.position;

        cursorWorldPos.z = 0;
        unitWorldPos.z = 0;

        float angle = Vector3.Angle(Vector3.up, cursorWorldPos - unitWorldPos);
        AttackDirection direction;

        if (angle <= 45)
        {
            direction = AttackDirection.Up;
        } else if (angle <= 135)
        {
            direction = Vector3.Cross(Vector3.up, cursorWorldPos - unitWorldPos).z < 0
                ? AttackDirection.Left
                : AttackDirection.Right;
        } else
        {
            direction = AttackDirection.Down;
        }

        if (CurrentUnit.AttackPattern.direction != direction)
        {
            CurrentUnit.AttackPattern.direction = direction;
            AvailableActionsChanged.Invoke();
        }
    }

    private void SelectUnit(Unit unit)
    {
        CurrentUnit = unit;
    }

    private IEnumerator PerformUnitAction(Vector3Int clickedPos, Unit clickedUnit)
    {
        if (ControlMode == ControlModes.Movement)
        {
            yield return StartCoroutine(PerformMovementAction(clickedPos));
        } else if (ControlMode == ControlModes.Attack)
        {
            bool actionWasPerformed = CurrentUnit.Attack(clickedPos, clickedUnit);
            if (actionWasPerformed) currentActionPoints -= 1;
        }
    }

    private IEnumerator PerformMovementAction(Vector3Int clickedPos)
    {
        bool isMovementClicked = CurrentUnit.availableMoves.Contains(clickedPos);
        if (isMovementClicked)
        {
            Unit actingUnit = CurrentUnit;
            SelectUnit(null);
            yield return StartCoroutine(actingUnit.Move(clickedPos));
            SelectUnit(actingUnit);
            currentActionPoints -= 1;
        }
    }
}
