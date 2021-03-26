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

public enum PlayerFaction
{
    Humans,
    Demons
}

[RequireComponent(typeof(SkillHandler))]
public class Unit : MonoBehaviour
{
    [SerializeField]
    private PlayerController _owner;
    public PlayerController Owner { get { return _owner; } }
    public UnitConfig unitClass;

    [HideInInspector]
    public SkillHandler skillHandler;

    [SerializeField]
    private HealthBar healthBar;

    [SerializeField]
    private StatusIcon statusIcon;

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

    private UnitStatus _inflictedStatus;
    public UnitStatus InflictedStatus
    {
        get { return _inflictedStatus; }
        private set {
            _inflictedStatus = value;
            statusIcon.SetValue(value);
        } 
    }

    private float _health;
    public float Health {
        get { return _health; }
        private set {
            _health = value;
            healthBar.SetValue(value, unitClass.BaseHealth);
        }
    }

    private float baseShield = 100f;
    public float Shield {
        get {
            int shieldReduction = InflictedStatus != null ? InflictedStatus.GetShieldReduction(unitClass.Type) : 0;
            int appliedPenalty = Mathf.Clamp(shieldReduction, 0, 100);
                return baseShield - appliedPenalty;
        }
        private set { baseShield = value; }
    }

    private void Awake()
    {
        // Assign references
        skillHandler = GetComponent<SkillHandler>();

        // Initial setup
        Owner.AddUnit(this);
        Health = unitClass.BaseHealth;
        spriteMaterial = sprite.material;
        sprite.sprite = unitClass.inGameSprites[Owner.faction];
        sprite.flipX = Owner.FacingLeft;

        // Register listeners
        AddListeners();
    }

    private void Start()
    {

        AlignToGrid();

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

    public SkillConfig GetSkillConfig(AttackModes mode)
    {
        Dictionary<AttackModes, SkillConfig> configsDict = new Dictionary<AttackModes, SkillConfig>()
        {
            { AttackModes.None, null },
            { AttackModes.Primary, unitClass.primarySkill },
            { AttackModes.Secondary, unitClass.secondarySkill },
        };

        return configsDict[mode];
    }

    public void ApplyDamage(float baseDamage, DamageConfig.Types type)
    {
        float amount = baseDamage * (1 + (100 - Shield) / 100);

        Health = Mathf.Clamp(Health - amount, 0, unitClass.BaseHealth);
        StartCoroutine(AnimateDamage(amount, type));

        if (Health == 0)
        {
            Kill();
        }
    }

    private void ApplyStatus()
    {
        if (InflictedStatus != null)
        {
            bool shouldRemoveStatus = InflictedStatus.OnTick(this);
            if (shouldRemoveStatus) RemoveStatus();
        }
    }

    public void InflictStatus(UnitStatus newStatus)
    {
        InflictedStatus = newStatus;
        newStatus.OnAdd(this);
    }

    public void RemoveStatus()
    {
        InflictedStatus = null;
    }

    private bool HasStatus(System.Type statusType)
    {
        return InflictedStatus != null && InflictedStatus.GetType() == statusType;
    }

    private void Kill()
    {
        RemoveListeners();
        gameObject.SetActive(false);
    }

    private IEnumerator AnimateDamage(float amount, DamageConfig.Types type)
    {
        animator.SetTrigger("Hit");
        damageText.color = DamageConfig.Colors[type];
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
                navigator.GetTile(CellPosition).OnUnitLeave(this);

                Vector3 currentStart = currentTarget == 1 ? startingPos : waypoints[currentTarget - 1];
                yield return StartCoroutine(CheckTurningAnimation(currentStart, waypoints[currentTarget]));

                float elapsedTime = 0f;
                while (elapsedTime < movementDuration)
                {
                    transform.position = Vector3.Lerp(currentStart, waypoints[currentTarget], (elapsedTime / movementDuration));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                bool canGoFurther = navigator.GetTile(CellPosition).OnUnitEnter(this);
                if (!canGoFurther)
                {
                    cellTargetPos = CellPosition;
                    targetPos = targetPos = navigator.CellToWorldPos(CellPosition);
                    break;
                }
            }

            tilesetTraversalProvider.ReserveNode(cellTargetPos);
            transform.position = targetPos;
            yield return AnimateFlip(Owner.FacingLeft);
        }
    }

    private IEnumerator CheckTurningAnimation(Vector3 moveFrom, Vector3 moveTo)
    {
        if (moveTo.x != moveFrom.x)
        {
            bool shouldFaceLeft = moveTo.x < moveFrom.x;
            yield return StartCoroutine(AnimateFlip(shouldFaceLeft));
        }
    }

    private IEnumerator AnimateFlip(bool shouldFaceLeft)
    {
        bool isFacingLeft = sprite.flipX;

        if (shouldFaceLeft != isFacingLeft)
        {
            yield return StartCoroutine(AnimateFlipShow(false));
            sprite.flipX = shouldFaceLeft;
            yield return StartCoroutine(AnimateFlipShow(true));
        }
    }

    private IEnumerator AnimateFlipShow(bool show)
    {
        float from = show ? 0 : 1f;
        float to = show ? 1f : 0;

        float duration = .1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            sprite.transform.localScale = new Vector3(Mathf.Lerp(from, to, (elapsedTime / duration)), 1f, 1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        sprite.transform.localScale = new Vector3(to, 1f, 1f);
    }

    public IEnumerator Attack(Vector3Int clickedPos, Unit clickedUnit)
    {
        SerializableDictionary<Vector3Int, AttackPatternField> fields = skillHandler.AttackArea;

        bool isAttackClicked = fields.ContainsKey(clickedPos) && fields[clickedPos] == AttackPatternField.On;
        if (isAttackClicked)
        {
            yield return StartCoroutine(skillHandler.ExecuteAttack(clickedPos, clickedUnit));
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
        int range = HasStatus(typeof(SwampedStatus)) && unitClass.SwampedMovementRange > 0 ? unitClass.SwampedMovementRange : unitClass.MovementRange;
        AvailableMoves = TilemapNavigator.Instance.CalculateMovementRange(CellPosition, range);
    }
}
