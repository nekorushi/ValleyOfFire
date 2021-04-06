using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum AttackModes
{
    None,
    Attack,
    MainAbility,
    SecondaryAbility
}

public class PlayerController : MonoBehaviour
{
    public UnityEvent UnitSelectionChanged;
    public UnityEvent ControlModeChanged;
    public UnityEvent AvailableActionsChanged;
    public UnityEvent TurnStarted;

    private Camera mainCamera;

    public PlayerTurnManager turnManager = new PlayerTurnManager();

    [Header("Player settings (for designers)")]
    [SerializeField]
    private string _playerName;
    public string PlayerName { get { return _playerName; } }

    public PlayerFaction faction;
    public bool FacingLeft = false;

    [SerializeField]
    private Color _playerColor;
    public Color PlayerColor { get { return _playerColor; } }

    private AttackModes _attackMode = AttackModes.None;
    public AttackModes AttackMode
    {
        get { return _attackMode; }
        private set
        {
            _attackMode = value;
            ControlModeChanged.Invoke();
        }
    }

    private List<Unit> _units = new List<Unit>();
    public List<Unit> Units { get { return _units; } }

    public void AddUnit(Unit unit)
    {
        _units.Add(unit);
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
    }

    private void Reset()
    {
        SelectUnit(null);
    }

    public bool HasAliveFireUnits {
        get {
            return Units
                .Where(unit => unit.unitClass.Type == UnitType.Fire && unit.Health > 0)
                .ToList()
                .Count > 0;
        }
    }

    public void ChangeAttackMode(AttackModes mode)
    {
        AttackMode = AttackMode == mode ? AttackModes.None : mode;
    }

    public IEnumerator PerformTurn()
    {
        turnManager.ResetTurn();
        TurnStarted.Invoke();

        while (!turnManager.TurnFinished)
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
        GameObject clickedObject = TilemapNavigator.Instance.GetObjectAtWorldPos(clickedWorldPos);

        if (clickedObject != null)
        {
            Unit clickedUnit = clickedObject.GetComponent<Unit>();

            if (CurrentUnit)
            {
                Vector3Int clickedCellPos = TilemapNavigator.Instance.WorldToCellPos(clickedWorldPos);
                yield return StartCoroutine(PerformUnitAction(clickedCellPos, clickedUnit));
                ChangeAttackMode(AttackModes.None);
            } else if (clickedUnit != null && Units.Contains(clickedUnit))
            {
                SelectUnit(clickedUnit);
            }
        }
    }

    private void SelectUnit(Unit unit)
    {
        bool clickedOwnUnit = unit != null && Units.Contains(unit);
        bool shouldUnselect = unit == null || CurrentUnit == unit || !turnManager.CanBeSelected(unit);

        CurrentUnit = shouldUnselect || !clickedOwnUnit ? null : unit;
    }

    private IEnumerator PerformUnitAction(Vector3Int clickedPos, Unit clickedUnit)
    {
        if (AttackMode == AttackModes.None)
        {
            if (clickedUnit == null)
            {
                yield return StartCoroutine(PerformMovementAction(clickedPos));
            } else
            {
                SelectUnit(clickedUnit);
            }
        } else
        {
            yield return StartCoroutine(PerformAttackAction(clickedPos, clickedUnit));
        }

        if (GameManager.Instance.CheckWinningConditions()) turnManager.FinishTurn();
    }

    private IEnumerator PerformMovementAction(Vector3Int clickedPos)
    {
        bool isMovementClicked = CurrentUnit.AvailableMoves.Contains(clickedPos);
        if (isMovementClicked)
        {
            bool usedAnActionPoint = turnManager.UseMovementPoint(CurrentUnit);
            if (usedAnActionPoint)
            {
                Unit actingUnit = CurrentUnit;
                SelectUnit(null);
                yield return StartCoroutine(actingUnit.Move(clickedPos));
                if (turnManager.CanPerformMovement(actingUnit) || turnManager.CanPerformAction(actingUnit))
                    SelectUnit(actingUnit);
            } else
            {
                SelectUnit(null);
            }
        } else
        {
            SelectUnit(null);
        }
    }

    private IEnumerator PerformAttackAction(Vector3Int clickedPos, Unit clickedUnit)
    {
        SkillConfig skillConfig = CurrentUnit.GetSkillConfig(AttackMode);

        Unit actingUnit = CurrentUnit;
        bool isAttackClicked = actingUnit.skillHandler.Contains(skillConfig, clickedPos);
        if (isAttackClicked)
        {

            LevelTile clickedTile = TilemapNavigator.Instance.GetTile(clickedPos);
            bool canAttack = skillConfig.CanPerformAttack(actingUnit, clickedUnit, clickedTile);
            if (canAttack)
            {
                bool usedAnActionPoint = turnManager.UseActionPoint(CurrentUnit);
                if (usedAnActionPoint)
                {
                    SelectUnit(null);
                    yield return StartCoroutine(actingUnit.Attack(skillConfig, clickedPos, clickedUnit));
                    if (turnManager.CanPerformMovement(actingUnit) || turnManager.CanPerformAction(actingUnit))
                        SelectUnit(actingUnit);
                }
            }
        }

        ChangeAttackMode(AttackModes.None);
        AvailableActionsChanged.Invoke();
    }
}
