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
[SelectionBase]
public class Unit : MonoBehaviour
{
    [Header("Unit settings (for designers)")]
    [SerializeField]
    private PlayerController _player;
    public PlayerController Player { get { return _player; } }
    public UnitConfig unitClass;

    [Header("Technical settings (for programmers)")]
    [HideInInspector] public SkillHandler skillHandler;

    [SerializeField] private ValueBar healthBar;
    [SerializeField] private SegmentedBar segmentedHealthBar;
    [SerializeField] private ValueBar shieldBar;
    [SerializeField] private TMP_Text damageText;

    private IValueBar activeHealthBar;

    public Animator animator;
    public Animator bgFxAnimator;
    public Animator fxAnimator;
    public Animator barFxAnimator;

    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private SpriteRenderer spriteBgFX;

    [SerializeField] private Sprite designerModeSprite;
    [SerializeField] private float spriteInGameYOffset;
    [SerializeField] private float spriteDesignerYOffset;

    private Material spriteMaterial;
    private AudioSource audioPlayer;

    private readonly float UNIT_Z_POSITION = -0.5f;
    private TilesetTraversalProvider tilesetTraversalProvider;
    public Vector3Int CellPosition { get { return TilemapNavigator.Instance.WorldToCellPos(transform.position); } }
    public List<Vector3Int> AvailableMoves { get; private set; }

    private HelpTooltipUser helpTooltip;

    [HideInInspector] public ResistancesManager resistancesManager;
    [HideInInspector] public StatusManager statusManager;

    private float _health;
    public float Health {
        get { return _health; }
        private set {
            _health = value;
            activeHealthBar.SetValue(value, unitClass.BaseHealth);

            if (unitClass.Type == UnitType.Guardian)
            {
                bgFxAnimator.SetBool("Burning", _health >= 2);
            }
        }
    }

    private float baseShield = 100f;
    public float Shield {
        get {
            int shieldReduction = statusManager.InflictedStatus != null
                ? statusManager.InflictedStatus.GetShieldReduction(unitClass.Type)
                : 0;
            int appliedPenalty = Mathf.Clamp(shieldReduction, 0, 100);
            float result = baseShield - appliedPenalty;
            return result;
        }
    }

    private void Awake()
    {
        // Assign references
        skillHandler = GetComponent<SkillHandler>();
        audioPlayer = GetComponent<AudioSource>();
        helpTooltip = GetComponent<HelpTooltipUser>();

        healthBar.gameObject.SetActive(false);

        if (unitClass.Type == UnitType.Guardian)
        {
            activeHealthBar = segmentedHealthBar;
            segmentedHealthBar.gameObject.SetActive(true);
            healthBar.gameObject.SetActive(false);
        } else
        {
            activeHealthBar = healthBar;
            segmentedHealthBar.gameObject.SetActive(false);
            healthBar.gameObject.SetActive(true);
        }

        // Initial setup
        Player.AddUnit(this);
        Health = unitClass.BaseHealth;
        resistancesManager = new ResistancesManager(this, unitClass.resistances);
        statusManager = new StatusManager(this, resistancesManager);
        spriteMaterial = sprite.material;
        GetComponentInChildren<ClassIcon>().SetValue(unitClass.Type);
        shieldBar.SetValue(Shield, baseShield);
        GraphicsToggle.Instance.DesignerModeChanged.AddListener(UpdateSprite);

        if (unitClass.Type == UnitType.Guardian)
        {
            bgFxAnimator.SetBool("Burning", true);
        }

        // Register listeners
        AddListeners();
    }

    private void Start()
    {
        AlignToGrid();

        tilesetTraversalProvider = UnitBlockManager.Instance.traversalProvider;
        tilesetTraversalProvider.ReserveNode(CellPosition);
    }

    private void OnValidate()
    {
        bool isPrefabInstance = transform.parent != null;
        if (isPrefabInstance)
        {
            string teamName = Player != null ? Player.PlayerName : "NoTeam";
            string unitName = unitClass != null ? unitClass.name : "NoClass";

            UpdateSprite();
            if (Player != null) FlipSprite(Player.FacingLeft);

            name = string.Format("{0}_{1}", teamName, unitName);
        }
    }

    public void Focus()
    {
        UpdateAvailableMoves();
        spriteMaterial.EnableKeyword("OUTBASE_ON");
    }

    public void Blur()
    {
        AvailableMoves = null;
        spriteMaterial.DisableKeyword("OUTBASE_ON");
    }

    private void UpdateSprite()
    {
        if (unitClass != null)
        {
            Sprite newSprite = GraphicsToggle.Instance.DesignerMode
                ? designerModeSprite
                : unitClass.inGameSprites[Player.faction];

            Color newColor = GraphicsToggle.Instance.DesignerMode
                ? unitClass.designerModeColors[Player.faction]
                : Color.white;
            newColor.a = 1;

            sprite.color = newColor;

            float newYPosition = GraphicsToggle.Instance.DesignerMode
                ? spriteDesignerYOffset
                : spriteInGameYOffset;

            sprite.transform.localPosition = new Vector3(
                sprite.transform.localPosition.x,
                newYPosition,
                sprite.transform.localPosition.z
            );

            sprite.sprite = newSprite;
            spriteBgFX.sprite = newSprite;
        }
    }

    private bool IsSpriteFlipped()
    {
        return sprite.flipX;
    }

    private void FlipSprite(bool shouldFlip)
    {
        sprite.flipX = shouldFlip;
        spriteBgFX.flipX = shouldFlip;
    }

    private void AddListeners()
    {
        if (Player) Player.TurnStarted.AddListener(statusManager.ApplyStatus);

        statusManager.StatusChanged += OnStatusChange;
    }

    private void RemoveListeners()
    {
        if (Player) Player.TurnStarted.RemoveListener(statusManager.ApplyStatus);

        statusManager.StatusChanged -= OnStatusChange;
    }

    private void OnStatusChange()
    {
        helpTooltip.tooltipText = statusManager.InflictedStatus != null ? statusManager.InflictedStatus.description : null;
        shieldBar.SetValue(Shield, baseShield);
    }

    public void SetDisabled(bool disabled)
    {
        if (disabled)
        {
            spriteMaterial.EnableKeyword("GREYSCALE_ON");
        } else
        {
            spriteMaterial.DisableKeyword("GREYSCALE_ON");
        }
    }

    public SkillConfig GetSkillConfig(AttackModes mode)
    {
        Dictionary<AttackModes, SkillConfig> configsDict = new Dictionary<AttackModes, SkillConfig>()
        {
            { AttackModes.None, null },
            { AttackModes.Attack, unitClass.baseAttack },
            { AttackModes.MainAbility, unitClass.mainAbility },
            { AttackModes.SecondaryAbility, unitClass.secondaryAbility },
        };

        return configsDict[mode];
    }

    public void PlaySound(AudioClip clip)
    {
        audioPlayer.PlayOneShot(clip);
    }

    public void ModifyHealth(DamageValue damageData)
    {
        float damageDealt = damageData.DamageDealt(this).value;

        if (damageDealt < 0) PlaySound(unitClass.damageTakenSound[Player.faction]);

        Health = Mathf.Clamp(
            Health + damageDealt,
            0,
            unitClass.BaseHealth
        );
        StartCoroutine(AnimateHealthChange(damageDealt, damageData));

        if (Health == 0) Kill();
        if (Player != null) Player.NotifyHealthChange();
    }
    private void Kill()
    {
        RemoveListeners();
        gameObject.SetActive(false);
    }

    private IEnumerator AnimateHealthChange(float damageDealt, DamageValue damageData)
    {
        animator.SetTrigger(damageData.type == DamageType.Heal ? "Heal" : "Hit");
        damageText.color = damageData.Color;
        damageText.text = Mathf.Abs(damageDealt).ToString();
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
            yield return StartCoroutine(Move(positionAfterPush.Value, 15, true));
        }
    }
     
    public IEnumerator Move(Vector3Int cellTargetPos, float movementSpeed = 3, bool skipTurning = false)
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
            navigator.GetTile(CellPosition).OnUnitLeave(this);

            for (int currentTarget = 1; currentTarget < waypoints.Count; currentTarget++)
            {
                Vector3 currentStart = currentTarget == 1 ? startingPos : waypoints[currentTarget - 1];
                if (!skipTurning)
                {
                    yield return StartCoroutine(CheckTurningAnimation(currentStart, waypoints[currentTarget]));
                }

                float elapsedTime = 0f;
                while (elapsedTime < movementDuration)
                {
                    transform.position = Vector3.Lerp(currentStart, waypoints[currentTarget], (elapsedTime / movementDuration));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            navigator.GetTile(CellPosition).OnUnitEnter(this);
            tilesetTraversalProvider.ReserveNode(cellTargetPos);
            transform.position = targetPos;
            yield return AnimateFlip(Player.FacingLeft);
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
        bool isFacingLeft = IsSpriteFlipped();

        if (shouldFaceLeft != isFacingLeft)
        {
            yield return StartCoroutine(AnimateFlipShow(false));
            FlipSprite(shouldFaceLeft);
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

    public IEnumerator Attack(SkillConfig config, Vector3Int clickedPos, Unit clickedUnit)
    {
        SerializableDictionary<Vector3Int, AttackPatternField> fields = skillHandler.AttackArea(config);

        bool isAttackClicked = fields.ContainsKey(clickedPos) && fields[clickedPos] == AttackPatternField.On;
        if (isAttackClicked)
        {
            yield return StartCoroutine(skillHandler.ExecuteAttack(config, clickedPos, clickedUnit));
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
        int range = statusManager.HasStatus(typeof(SwampedStatus)) && unitClass.SwampedMovementRange > 0 ? unitClass.SwampedMovementRange : unitClass.MovementRange;
        AvailableMoves = TilemapNavigator.Instance.CalculateMovementRange(CellPosition, range);
    }
}
