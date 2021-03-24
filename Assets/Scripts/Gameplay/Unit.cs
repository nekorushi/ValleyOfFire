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

[RequireComponent(typeof(Skill))]
public class Unit : MonoBehaviour
{
    public PlayerController Owner { get; private set; }
    public Skill PrimaryAttack { get; private set; }
    public Skill SecondaryAttack { get; private set; }
    [SerializeField]
    private TMP_Text healthText;
    [SerializeField]
    private TMP_Text damageText;
    public Animator animator;
    public Animator fxAnimator;

    [SerializeField]
    private SpriteRenderer sprite;
    private Material spriteMaterial;

    private readonly float UNIT_Z_POSITION = -0.5f;
    private TilesetTraversalProvider tilesetTraversalProvider;
    public Vector3Int CellPosition { get { return TilemapNavigator.Instance.WorldToCellPos(transform.position); } }

    public List<Vector3Int> AvailableMoves { get; private set; }
    private UnitStatus inflictedStatus;

    [Header("Unit settings")]
    [SerializeField]
    private UnitTypes _unitType;
    public UnitTypes UnitType { get { return _unitType; } private set { _unitType = value; } }

    [SerializeField]
    private float _health = 5f;
    public float Health { get { return _health; } private set { _health = value; } }

    private float baseShield = 100f;
    public float Shield {
        get {
            int shieldReduction = inflictedStatus != null ? inflictedStatus.GetShieldReduction(UnitType) : 0;
            int appliedPenalty = Mathf.Clamp(shieldReduction, 0, 100);
                return baseShield - appliedPenalty;
        }
        private set { baseShield = value; }
    }

    [SerializeField]
    private int movementRange;
    [SerializeField]
    private int swampedMovementRange;

    public void SetOwner(PlayerController player)
    {
        RemoveListeners();
        Owner = player;
        AddListeners();

        sprite.material.SetColor("_Color", player.PlayerColor);
    }

    private void Awake()
    {
        Skill[] attackPatterns = GetComponents<Skill>();

        PrimaryAttack = attackPatterns[0];
        SecondaryAttack = attackPatterns[1];

        spriteMaterial = sprite.material;
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
            Owner.TurnStarted.AddListener(ApplyStatus);
        }
    }

    private void RemoveListeners()
    {
        if (Owner)
        {
            Owner.TurnStarted.RemoveListener(ApplyStatus);
        }
    }

    public void Focus() {
        UpdateAvailableMoves();

        spriteMaterial.EnableKeyword("OUTBASE_ON");
    }

    public void Blur()
    {
        AvailableMoves = null;

        spriteMaterial.DisableKeyword("OUTBASE_ON");
    }

    public Skill GetAttackPattern(AttackModes mode)
    {
        if (mode == AttackModes.Primary) return PrimaryAttack;
        return SecondaryAttack;
    }

    public void ApplyDamage(float baseDamage, DamageType type)
    {
        float amount = baseDamage * (1 + (100 - Shield) / 100);

        Health = Mathf.Clamp(Health - amount, 0, Health);
        UpdateHealthText();
        StartCoroutine(AnimateDamage(amount, type));

        if (Health == 0)
        {
            Kill();
        }
    }

    private void ApplyStatus()
    {
        if (inflictedStatus != null)
        {
            bool shouldRemoveStatus = inflictedStatus.OnTick(this);
            if (shouldRemoveStatus) RemoveStatus();
        }
    }

    public void InflictStatus(UnitStatus newStatus)
    {
        inflictedStatus = newStatus;
        newStatus.OnAdd(this);
    }

    public void RemoveStatus()
    {
        inflictedStatus = null;
    }

    private bool HasStatus(System.Type statusType)
    {
        return inflictedStatus != null && inflictedStatus.GetType() == statusType;
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

    private IEnumerator AnimateDamage(float amount, DamageType type)
    {
        Dictionary<DamageType, Color> dmgColors = new Dictionary<DamageType, Color>()
        {
            { DamageType.Fire, new Color(1, .5f, 0) },
            { DamageType.Normal, Color.red },
            { DamageType.Heal, Color.green }
        };

        animator.SetTrigger("Hit");
        damageText.color = dmgColors[type];
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

            if (waypoints.Count > 0)
            {
                LevelTile reachedTile = navigator.GetTile(CellPosition);
                reachedTile.OnUnitLeave(this);
            }

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

                LevelTile reachedTile = navigator.GetTile(CellPosition);
                bool canGoFurther = reachedTile.OnUnitEnter(this);
                if (!canGoFurther)
                {
                    cellTargetPos = CellPosition;
                    targetPos = targetPos = navigator.CellToWorldPos(CellPosition);
                    break;
                }
            }

            tilesetTraversalProvider.ReserveNode(cellTargetPos);
            transform.position = targetPos;
        }
    }

    public IEnumerator Attack(Vector3Int clickedPos, Unit clickedUnit)
    {
        Skill attackPattern = GetAttackPattern(Owner.AttackMode);
        SerializableDictionary<Vector3Int, AttackPatternField> fields = attackPattern.AttackArea;

        bool isAttackClicked = fields.ContainsKey(clickedPos) && fields[clickedPos] == AttackPatternField.On;
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
        int range = HasStatus(typeof(SwampedStatus)) && swampedMovementRange > 0 ? swampedMovementRange : movementRange;
        AvailableMoves = TilemapNavigator.Instance.CalculateMovementRange(CellPosition, range);
    }
}
