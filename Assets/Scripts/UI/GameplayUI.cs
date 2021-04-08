using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Tilemaps;

enum MarkerTypes
{
    Default,
    Movement,
    MovementPreview,
    Attack,
    AttackPreview,
}

public class GameplayUI : MonoBehaviour
{
    private static GameplayUI _instance;
    public static GameplayUI Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<GameplayUI>();
            return _instance;
        }
    }

    public UnityEvent ActivePlayerChanged;

    private Camera mainCamera;
    private RectTransform canvasRect;

    TilemapNavigator navigator;

    [SerializeField] private Tilemap movementAreaTilemap;
    [SerializeField]  private Tilemap attackAreaTilemap;

    [HideInInspector] private List<Vector3Int> availableMoves = new List<Vector3Int>();
    [HideInInspector] private List<Vector3Int> availableAttacks = new List<Vector3Int>();

    [SerializeField] private Color defaultTileTint = Color.white;
    [SerializeField] private Color movementTileTint = Color.blue;
    [SerializeField] private Color movementPreviewTileTint = Color.cyan;
    [SerializeField] private Color attackTileTint = Color.red;
    [SerializeField] private Color attackPreviewTileTint = Color.magenta;

    [SerializeField]
    private GameObject dmgFormulaPrefab;
    private List<GameObject> dmgFormulas = new List<GameObject>();

    [SerializeField]
    private SelectedUnitPanel selectedUnitPanel;

    [SerializeField]
    private UnitTooltip unitTooltip;

    [SerializeField]
    private List<PlayerController> allPlayers;

    private Unit _hoveredUnit;
    public Unit HoveredUnit
    {
        get { return _hoveredUnit; }
        set
        {
            unitTooltip.SetUnit(value);

            if (ActivePlayer.AttackMode == AttackModes.None)
            {
                _hoveredUnit = value;
                UpdateAvailableActions();
            }
        }
    }

    private Vector3Int _hoveredCell;
    public Vector3Int HoveredCell
    {
        get { return _hoveredCell; }
        set
        {
            _hoveredCell = value;
            UpdateAvailableActions();

            Unit unit = navigator.GetUnit(value);
            if (HoveredUnit != unit) HoveredUnit = unit;
        }
    }

    private PlayerController _activePlayer;
    public PlayerController ActivePlayer {
        get { return _activePlayer; }
        set
        {
            DisconnectPlayer();
            _activePlayer = value;
            ConnectPlayer();
            ActivePlayerChanged.Invoke();
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        canvasRect = GetComponent<RectTransform>();
        navigator = TilemapNavigator.Instance;
        ResetTint(movementAreaTilemap);
        ResetTint(attackAreaTilemap);

        InvokeRepeating("HandleMouseHover", 0f, .1f);
    }
    private void HandleMouseHover()
    {
        Vector3Int hoveredPosition = navigator.WorldToCellPos(
            mainCamera.ScreenToWorldPoint(Input.mousePosition)
        );

        if (HoveredCell == null || HoveredCell != hoveredPosition)
        {
            HoveredCell = hoveredPosition;
        }
    }

    void ConnectPlayer()
    {
        if (ActivePlayer != null)
        {
            ActivePlayer.UnitSelectionChanged.AddListener(UpdateUnitSelection);
            ActivePlayer.ControlModeChanged.AddListener(UpdateAvailableActions);
            ActivePlayer.AvailableActionsChanged.AddListener(UpdateAvailableActions);
        }
    }

    void DisconnectPlayer()
    {
        if (ActivePlayer != null)
        {
            ActivePlayer.UnitSelectionChanged.RemoveListener(UpdateUnitSelection);
            ActivePlayer.ControlModeChanged.RemoveListener(UpdateAvailableActions);
            ActivePlayer.AvailableActionsChanged.RemoveListener(UpdateAvailableActions);
        }
    }

    void UpdateUnitSelection()
    {
        UpdateUnitPanel();
        UpdateAvailableActions();
    }

    void UpdateUnitPanel()
    {
        selectedUnitPanel.UpdateUnit(ActivePlayer);
    }

    void UpdateAvailableActions()
    {
        ClearAvailableMoves();
        ClearAvailableAttacks();
        ClearDamageFormulas();


        Unit unit = HoveredUnit != null ? HoveredUnit : ActivePlayer.CurrentUnit;
        if (unit != null)
        {
            if (ShouldShowMovementRange(unit)) RenderAvailableMoves(unit);
            if (ShouldShowAttackRange(unit)) RenderAvailableAttacks(unit);
            if (ShouldShowDmgFormulas(unit)) RenderDmgFormulas(unit);
        }
    }

    bool ShouldShowMovementRange(Unit unit)
    {
        bool selectedUnitRange = 
            ActivePlayer.AttackMode == AttackModes.None
            && unit == ActivePlayer.CurrentUnit 
            && ActivePlayer.turnManager.CanPerformMovement(unit);
        if (selectedUnitRange) return true;

        bool alliedUnit = unit != ActivePlayer.CurrentUnit 
            && unit.Player == ActivePlayer 
            && ActivePlayer.turnManager.CanPerformMovement(unit);
        if (alliedUnit) return true;

        bool enemyUnit = unit.Player != ActivePlayer;
        return enemyUnit;
    }

    bool ShouldShowAttackRange(Unit unit)
    {
        bool selectedUnitRange =
            ActivePlayer.AttackMode != AttackModes.None
            && unit == ActivePlayer.CurrentUnit
            && ActivePlayer.turnManager.CanPerformAction(unit);
        if (selectedUnitRange) return true;

        bool alliedUnit = unit.Player == ActivePlayer && ActivePlayer.turnManager.CanPerformAction(unit);
        if (alliedUnit) return true;

        bool enemyUnit = unit.Player != ActivePlayer;
        return enemyUnit;
    }

    bool ShouldShowDmgFormulas(Unit unit)
    {
        bool selectedUnitRange =
            ActivePlayer.AttackMode != AttackModes.None
            && unit == ActivePlayer.CurrentUnit
            && ActivePlayer.turnManager.CanPerformAction(unit);
        
        return selectedUnitRange;
    }

    void ClearAvailableMoves()
    {
        availableMoves.ForEach(position => TintMarker(movementAreaTilemap, position, MarkerTypes.Default));
        availableMoves.Clear();
    }

    void RenderAvailableMoves(Unit unit)
    {
        if (unit != null)
        {
            if (unit.AvailableMoves == null) unit.UpdateAvailableMoves();
            unit.AvailableMoves.ForEach(position =>
            {
                MarkerTypes markerType = unit == ActivePlayer.CurrentUnit && ActivePlayer.turnManager.CanPerformMovement(unit)
                    ? MarkerTypes.Movement
                    : MarkerTypes.MovementPreview;
                TintMarker(movementAreaTilemap, position, markerType);
                availableMoves.Add(position);
            });
        }
    }

    void ClearAvailableAttacks()
    {
        availableAttacks.ForEach(position => TintMarker(attackAreaTilemap, position, MarkerTypes.Default));
        availableAttacks.Clear();
    }

    void RenderAvailableAttacks(Unit unit)
    {
        if (unit == null) return;

        SkillConfig skillConfig = unit.GetSkillConfig(unit == ActivePlayer.CurrentUnit 
            ? ActivePlayer.AttackMode 
            : AttackModes.Attack
        );
        if (skillConfig == null) return;

        TilemapNavigator navigator = TilemapNavigator.Instance;
        SerializableDictionary<Vector3Int, AttackPatternField> pattern = unit.skillHandler.AttackArea(skillConfig);
        if (pattern != null)
        {
            foreach (KeyValuePair<Vector3Int, AttackPatternField> field in pattern)
            {
                LevelTile targetTile = navigator.GetTile(field.Key);
                bool hasTile = targetTile != null;
                Unit targetUnit = navigator.GetUnit(field.Key);

                if (field.Value == AttackPatternField.On && hasTile)
                {
                    bool canAttack = skillConfig.CanPerformAttack(unit, targetUnit, targetTile);
                    TintMarker(attackAreaTilemap, field.Key, canAttack && unit != HoveredUnit ? MarkerTypes.Attack : MarkerTypes.AttackPreview);
                    availableAttacks.Add(field.Key);
                }
            }
        }
    }

    void ClearDamageFormulas()
    {
        dmgFormulas.ForEach(image => Destroy(image));
        dmgFormulas.Clear();
    }

    void RenderDmgFormulas(Unit unit)
    {
        if (unit == null) return;

        foreach(PlayerController player in allPlayers)
        {
            if (player != unit.Player)
            {
                foreach(Unit defender in player.Units)
                {
                    Unit attacker = unit;
                    if (defender != null && defender.Player != unit.Player && defender.Health > 0)
                    {
                        SkillConfig skillConfig = unit.GetSkillConfig(unit.Player.AttackMode);
                        GameObject formula = CreateDmgFormula(
                            defender.CellPosition,
                            new DamageValue(
                                skillConfig.baseDamage,
                                UnitsConfig.Instance.GetExtraDamage(attacker.unitClass.Type, defender.unitClass.Type),
                                DamageType.Normal,
                                DamageTrajectory.SelfInflicted
                            ),
                            defender.unitClass.Type
                        );
                        dmgFormulas.Add(formula);
                    }
                }
            }
        }
    }

    private void TintMarker(Tilemap tilemap, Vector3Int position, MarkerTypes type)
    {
        Dictionary<MarkerTypes, Color> markerColors = new Dictionary<MarkerTypes, Color>()
        {
            {MarkerTypes.Default, defaultTileTint },
            {MarkerTypes.Attack, attackTileTint },
            {MarkerTypes.AttackPreview, attackPreviewTileTint },
            {MarkerTypes.Movement, movementTileTint },
            {MarkerTypes.MovementPreview, movementPreviewTileTint }
        };

        Color tintColor = markerColors[type];

        bool shouldHighlight = position == HoveredCell && (type == MarkerTypes.Attack || type == MarkerTypes.Movement);
        if (shouldHighlight) tintColor.a = 180; 

        tilemap.SetColor(position, tintColor);
    }

    private void ResetTint(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                for (int z = bounds.min.z; z < bounds.max.z; z++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, z);
                    if (tilemap.HasTile(cellPos))
                    {
                        TintMarker(tilemap, cellPos, MarkerTypes.Default);
                    }
                }
            }

        }
    }

    private GameObject CreateDmgFormula(Vector3Int position, DamageValue damage, UnitType defenderClass)
    {
        Vector3 worldPos = TilemapNavigator.Instance.CellToWorldPos(position);
        Vector2 canvasPos = WorldToCanvasPos(worldPos);

        GameObject formula = Instantiate(dmgFormulaPrefab);
        TMP_Text value = formula.GetComponentInChildren<TMP_Text>();

        string messageFormat = damage.extraFlatDamage == 0f
            ? "{0}"
            : damage.extraFlatDamage > 0 ? "{0}\n(+{1})" : "{0}\n({1})";

        value.text = string.Format(messageFormat,  damage.baseFlatDmg, damage.extraFlatDamage, defenderClass);

        RectTransform markerRect = formula.GetComponent<RectTransform>();
        markerRect.SetParent(canvasRect);
        markerRect.anchoredPosition = canvasPos;

        return formula;
    }

    private Vector2 WorldToCanvasPos(Vector3 worldPos)
    {
        Vector2 viewportPos = mainCamera.WorldToViewportPoint(worldPos);
        return new Vector2(
            viewportPos.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f,
            viewportPos.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f
        );
    }
}
