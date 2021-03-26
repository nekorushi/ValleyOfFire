using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum AttackModes
{
    None,
    Primary,
    Secondary
}

public class PlayerController : MonoBehaviour
{
    public UnityEvent UnitSelectionChanged;
    public UnityEvent ControlModeChanged;
    public UnityEvent AvailableActionsChanged;
    public UnityEvent TurnStarted;

    [Header("Component configuration (for programmers)")]
    [SerializeField]
    private Camera mainCamera;
    private byte currentActionPoints = 0;

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
            if (CurrentUnit) CurrentUnit.skillHandler.config = CurrentUnit.GetSkillConfig(AttackMode);
            ControlModeChanged.Invoke();
        }
    }

    [SerializeField]
    private byte maxActionPoints = 2;

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

    public bool HasAliveUnits {
        get {
            return Units
                .Where(unit => unit.Health > 0)
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
        TurnStarted.Invoke();
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
        GameObject clickedObject = TilemapNavigator.Instance.GetObjectAtWorldPos(clickedWorldPos);

        if (clickedObject != null)
        {
            Unit clickedUnit = clickedObject.GetComponent<Unit>();

            if (CurrentUnit)
            {
                Vector3Int clickedCellPos = TilemapNavigator.Instance.WorldToCellPos(clickedWorldPos);
                yield return StartCoroutine(PerformUnitAction(clickedCellPos, clickedUnit));
                ChangeAttackMode(AttackModes.None);
            } else if (clickedUnit != null)
            {
                SelectUnit(clickedUnit);
            }
        }
    }

    private void SelectUnit(Unit unit)
    {
        CurrentUnit = unit;
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
                yield return StartCoroutine(PerformUnitSelect(clickedUnit));
            }
        } else
        {
            yield return StartCoroutine(PerformAttackAction(clickedPos, clickedUnit));
        }
    }

    private IEnumerator PerformUnitSelect(Unit clickedUnit)
    {
        bool clickedOwnUnit = clickedUnit != null && Units.Contains(clickedUnit);
        bool shouldUnselect = CurrentUnit == clickedUnit;

        SelectUnit(shouldUnselect || !clickedOwnUnit ? null : clickedUnit);
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator PerformMovementAction(Vector3Int clickedPos)
    {
        bool isMovementClicked = CurrentUnit.AvailableMoves.Contains(clickedPos);
        if (isMovementClicked)
        {
            Unit actingUnit = CurrentUnit;
            SelectUnit(null);
            yield return StartCoroutine(actingUnit.Move(clickedPos));
            SelectUnit(actingUnit);
            currentActionPoints -= 1;
        } else
        {
            SelectUnit(null);
        }
    }

    private IEnumerator PerformAttackAction(Vector3Int clickedPos, Unit clickedUnit)
    {
        Unit actingUnit = CurrentUnit;
        bool isAttackClicked = actingUnit.skillHandler.Contains(clickedPos);
        if (isAttackClicked)
        {
            AttackTargets allowedTargets = actingUnit.skillHandler.config.targets;
            bool isSkillAvailable = actingUnit.skillHandler.config.isActive;
            bool canAttackAllies = allowedTargets == AttackTargets.Allies;
            bool canAttackEnemies = allowedTargets == AttackTargets.Enemies;
            bool canAttackSameClassAlly = allowedTargets == AttackTargets.SameClassAlly;
            bool canAttackEnemiesAndSameClassAlly = allowedTargets == AttackTargets.EnemiesOrSameClassAlly;

            bool clickedAlly = clickedUnit != actingUnit && Units.Contains(clickedUnit);
            bool clickedEnemy = !Units.Contains(clickedUnit);
            bool clickedSameClassAlly = clickedAlly && clickedUnit.unitClass.Type == actingUnit.unitClass.Type;
            bool clickedEnemyOrSameClassAlly = clickedEnemy || clickedSameClassAlly;

            LevelTile clickedTile = TilemapNavigator.Instance.GetTile(clickedPos);

            bool canAttackEnvironment = clickedTile != null && clickedTile.CanBeAttacked;
            bool canAttackUnit = clickedUnit != null
                && (
                    allowedTargets == AttackTargets.Both
                    || canAttackAllies && clickedAlly
                    || canAttackEnemies && clickedEnemy
                    || canAttackSameClassAlly && clickedSameClassAlly
                    || canAttackEnemiesAndSameClassAlly && clickedEnemyOrSameClassAlly
                );
            bool canAttack = isSkillAvailable && (canAttackUnit || canAttackEnvironment);

            if (canAttack)
            {
                SelectUnit(null);
                yield return StartCoroutine(actingUnit.Attack(clickedPos, clickedUnit));
                SelectUnit(actingUnit);
                currentActionPoints -= 1;
            }
        }

        ChangeAttackMode(AttackModes.None);
        AvailableActionsChanged.Invoke();
    }
}
