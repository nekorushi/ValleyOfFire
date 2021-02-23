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
                if (currentUnit) yield return StartCoroutine(PerformUnitAction(clickedCellPos));
            }
        }
    }

    private void SelectUnit(Unit unit)
    {
        currentUnit = unit;
    }

    private IEnumerator PerformUnitAction(Vector3Int clickedPos)
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
            }

            currentActionPoints -= 1;
        }
    }
}
