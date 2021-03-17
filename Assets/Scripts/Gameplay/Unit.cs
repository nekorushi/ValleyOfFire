using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Pathfinding;

enum MovementType
{
    Straight,
    Diagonal,
    Both
}

[RequireComponent(typeof(AttackPattern))]
public class Unit : MonoBehaviour
{
    public PlayerController Owner { get; private set; }
    public AttackPattern PrimaryAttack { get; private set; }
    public AttackPattern SecondaryAttack { get; private set; }
    [SerializeField]
    private TMP_Text healthText;
    [SerializeField]
    private TMP_Text damageText;
    [SerializeField]
    public Animator animator;
    [SerializeField]
    public Animator fxAnimator;

    private readonly float UNIT_Z_POSITION = -0.5f;
    private TilesetTraversalProvider tilesetTraversalProvider;
    public Vector3Int CellPosition { get { return TilemapNavigator.Instance.WorldToCellPos(transform.position); } }

    public List<Vector3Int> AvailableMoves { get; private set; }
    private List<UnitStatus> statuses = new List<UnitStatus>();

    [Header("Unit settings")]
    [SerializeField]
    private UnitTypes _unitType;
    public UnitTypes UnitType { get { return _unitType; } private set { _unitType = value; } }

    [SerializeField]
    private float _health = 5f;
    public float Health { get { return _health; } private set { _health = value; } }

    private float _shield = 100f;
    public float Shield { get { return _shield; } private set { _shield = value; } }

    [SerializeField]
    private int movementRange;

    public void SetOwner(PlayerController player)
    {
        RemoveListeners();
        Owner = player;
        AddListeners();

        SpriteRenderer unitSprite = GetComponentInChildren<SpriteRenderer>();
        if (unitSprite) unitSprite.color = player.PlayerColor;
    }

    private void Awake()
    {
        AttackPattern[] attackPatterns = GetComponents<AttackPattern>();

        PrimaryAttack = attackPatterns[0];
        SecondaryAttack = attackPatterns[1];
    }

    private void Start()
    {

        AlignToGrid();
        UpdateHealthText();

        tilesetTraversalProvider = UnitBlockManager.Instance.traversalProvider;
        tilesetTraversalProvider.ReserveNode(CellPosition);

    }

    private void AddListeners()
    {
        if (Owner)
        {
            Owner.TurnStarted.AddListener(ApplyStatuses);
        }
    }

    private void RemoveListeners()
    {
        if (Owner)
        {
            Owner.TurnStarted.RemoveListener(ApplyStatuses);
        }
    }

    public void Focus() {
        UpdateAvailableMoves();
    }

    public void Blur()
    {
        AvailableMoves = null;
    }

    public AttackPattern GetAttackPattern(AttackModes mode)
    {
        if (mode == AttackModes.Primary) return PrimaryAttack;
        return SecondaryAttack;
    }

    public void ApplyDamage(float baseDamage)
    {
        float amount = baseDamage * (1 + (100 - Shield) / 100);

        Health = Mathf.Clamp(Health - amount, 0, Health);
        UpdateHealthText();
        StartCoroutine(AnimateDamage(amount));

        if (Health == 0)
        {
            Kill();
        }
    }

    private void ApplyStatuses()
    {
        List<UnitStatus> toRemove = new List<UnitStatus>();
        foreach(UnitStatus status in statuses)
        {
            bool shouldRemoveStatus = status.OnTick(this);
            if (shouldRemoveStatus) toRemove.Add(status);
        }

        foreach(UnitStatus status in toRemove)
        {
            statuses.Remove(status);
        }
    }

    public void AddStatus(UnitStatus newStatus)
    {
        Debug.Log(newStatus.GetType());
        statuses.Add(newStatus);
        newStatus.OnAdd(this);
    }

    private void Kill()
    {
        RemoveListeners();
        gameObject.SetActive(false);
    }

    private void UpdateHealthText()
    {
        healthText.text = Health.ToString("N1");
    }

    private IEnumerator AnimateDamage(float amount)
    {
        animator.SetTrigger("Hit");
        damageText.text = amount.ToString();
        damageText.gameObject.SetActive(true);

        for (float current = 0; current < 1f; current += 0.1f)
        {
            damageText.rectTransform.anchoredPosition = new Vector2(damageText.rectTransform.anchoredPosition.x, current);
            yield return new WaitForSeconds(.03f);
        }

        damageText.gameObject.SetActive(false);
    }

    public IEnumerator Push(WorldUtils.Direction direction, int distance)
    {
        TilemapNavigator navigator = TilemapNavigator.Instance;
        Vector3Int step = new Dictionary<WorldUtils.Direction, Vector3Int> {
            { WorldUtils.Direction.Up, Vector3Int.up },
            { WorldUtils.Direction.Down, Vector3Int.down },
            { WorldUtils.Direction.Left, Vector3Int.left },
            { WorldUtils.Direction.Right, Vector3Int.right },
        }[direction];

        Vector3Int? positionAfterPush = null;

        for (int i = 1; i <= distance; i++)
        {
            Vector3Int testedSpot = CellPosition + step * i;
            if (navigator.IsTileWalkable(testedSpot) && !navigator.IsTileTaken(testedSpot))
            {
                positionAfterPush = testedSpot;
            } else
            {
                break;
            }
        }

        if (positionAfterPush.HasValue)
        {
            yield return StartCoroutine(Move(positionAfterPush.Value, 15));
        }
    }

    public IEnumerator Move(Vector3Int cellTargetPos, float movementSpeed = 3)
    {
        TilemapNavigator navigator = TilemapNavigator.Instance;

        Vector3 startingPos = transform.position;
        Vector3 targetWorldPos = navigator.CellToWorldPos(cellTargetPos);
        Vector3 targetPos = new Vector3(targetWorldPos.x, targetWorldPos.y, UNIT_Z_POSITION);

        float movementDuration = 1 / movementSpeed;

        UnitBlockManager.Instance.traversalProvider.selectedUnitCell = CellPosition;
        Path path = ABPath.Construct(CellPosition, cellTargetPos, null);
        path.traversalProvider = tilesetTraversalProvider;
        AstarPath.StartPath(path);

        yield return StartCoroutine(path.WaitForPath());
        if (!path.error)
        {
            tilesetTraversalProvider.ReleaseNode(CellPosition);
            List<Vector3> waypoints = path.vectorPath.ConvertAll(node => navigator.CellToWorldPos(navigator.WorldToCellPos(node)));

            for (int currentTarget = 1; currentTarget < waypoints.Count; currentTarget++)
            {
                float elapsedTime = 0f;
                Vector3 currentStart = currentTarget == 1 ? startingPos : waypoints[currentTarget - 1];
                while (elapsedTime < movementDuration)
                {
                    transform.position = Vector3.Lerp(currentStart, waypoints[currentTarget], (elapsedTime / movementDuration));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            tilesetTraversalProvider.ReserveNode(cellTargetPos);
            transform.position = targetPos;
        }
    }

    public IEnumerator Attack(Vector3Int clickedPos, Unit clickedUnit)
    {
        Vector3Int clickRelativePos = clickedPos - CellPosition;
        Vector2Int clickRelativePos2D = new Vector2Int(clickRelativePos.x, clickRelativePos.y);
        AttackPattern attackPattern = GetAttackPattern(Owner.AttackMode);
        SerializableDictionary<Vector2Int, AttackPatternField> fields = attackPattern.Pattern;

        bool isAttackClicked = fields.ContainsKey(clickRelativePos2D) && fields[clickRelativePos2D] == AttackPatternField.On;
        if (isAttackClicked)
        {
            yield return StartCoroutine(attackPattern.ExecuteAttack(clickedPos, clickedUnit));
        }
    }

    private void AlignToGrid()
    {
        TilemapNavigator navigator = TilemapNavigator.Instance;
        Vector3 alignedPosition = navigator.CellToWorldPos(navigator.WorldToCellPos(transform.position));

        transform.position = new Vector3(alignedPosition.x, alignedPosition.y, UNIT_Z_POSITION);
    }

    public void UpdateAvailableMoves()
    {
        AvailableMoves = TilemapNavigator.Instance.CalculateMovementRange(CellPosition, movementRange);
    }
}
