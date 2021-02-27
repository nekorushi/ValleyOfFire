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
    private List<Unit> units;

    private Unit _currentUnit;  
    public Unit currentUnit {
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
        ColorizeUnits();
    }

    private void Reset()
    {
        SelectUnit(null);
    }

    public bool HasAliveUnits { get { return units.Where(unit => unit.Health > 0).ToList().Count > 0; } }
    

    private void ColorizeUnits()
    {
        units.ForEach(unit =>
        {
            if (unit != null)
            {
                SpriteRenderer unitSprite = unit.gameObject.GetComponentInChildren<SpriteRenderer>();
                if (unitSprite) unitSprite.color = PlayerColor;
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
            if (currentUnit && currentUnit.AttackPattern.attackType == AttackType.Area)
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
        RaycastHit2D hit = Physics2D.Raycast(clickedWorldPos, Vector2.zero, Mathf.Infinity, layerMask);

        if (hit.collider != null)
        {
            Unit clickedUnit = hit.collider.gameObject.GetComponent<Unit>();
            bool clickedOwnUnit = units.Contains(clickedUnit);

            if (clickedOwnUnit)
            {
                bool shouldUnselect = currentUnit == clickedUnit;
                SelectUnit(shouldUnselect ? null : clickedUnit);
            } else
            {
                Vector3Int clickedCellPos = TilemapNavigator.Instance.WorldToCellPos(clickedWorldPos);
                if (currentUnit) yield return StartCoroutine(PerformUnitAction(clickedCellPos, clickedUnit));
            }
        }
    }

    private void UpdateAttackArea()
    {
        Vector3 cursorWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 unitWorldPos = currentUnit.transform.position;

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

        if (currentUnit.AttackPattern.direction != direction)
        {
            currentUnit.AttackPattern.direction = direction;
            AvailableActionsChanged.Invoke();
        }
    }

    private void SelectUnit(Unit unit)
    {
        currentUnit = unit;
    }

    private IEnumerator PerformUnitAction(Vector3Int clickedPos, Unit clickedUnit)
    {
        if (ControlMode == ControlModes.Movement)
        {
            bool isMovementClicked = currentUnit.availableMoves.Contains(clickedPos);
            if (isMovementClicked)
            {
                Unit actingUnit = currentUnit;
                SelectUnit(null);
                yield return StartCoroutine(actingUnit.Move(clickedPos));
                SelectUnit(actingUnit);
                currentActionPoints -= 1;
            }
        } else if (ControlMode == ControlModes.Attack)
        {
            Vector3Int clickRelativePos = clickedPos - currentUnit.CellPosition;
            Vector2Int clickRelativePos2D = new Vector2Int(clickRelativePos.x, clickRelativePos.y);
            SerializableDictionary<Vector2Int, AttackPatternField> fields = currentUnit.AttackPattern.sourcePattern;
            bool isAttackClicked = fields.ContainsKey(clickRelativePos2D) && fields[clickRelativePos2D] == AttackPatternField.On;
            if (isAttackClicked && clickedUnit != null)
            {
                clickedUnit.ApplyDamage(currentUnit.AttackDmg);
                currentActionPoints -= 1;
            }
        }
    }
}
