using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;

enum MarkerTypes
{
    Default,
    Movement,
    MovementPreview,
    Attack,
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

    [SerializeField] private Tilemap movementAreaTilemap;
    [SerializeField]  private Tilemap attackAreaTilemap;

    [HideInInspector] private List<Vector3Int> availableMoves = new List<Vector3Int>();
    [HideInInspector] private List<Vector3Int> availableAttacks = new List<Vector3Int>();

    [SerializeField] private Color defaultTileTint = Color.white;
    [SerializeField] private Color movementTileTint = Color.green;
    [SerializeField] private Color movementPreviewTileTint = Color.cyan;
    [SerializeField] private Color attackTileTint = Color.red;

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
            _hoveredUnit = value;
            UpdateAvailableActions();
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
        ResetTint(movementAreaTilemap);
        ResetTint(attackAreaTilemap);
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

    void UpdateAvailableActions()
    {
        ClearAvailableMoves();
        ClearAvailableAttacks();
        ClearDamageFormulas();

        if (ActivePlayer.AttackMode == AttackModes.None)
        {            
            RenderAvailableMoves(HoveredUnit != null ? HoveredUnit : ActivePlayer.CurrentUnit);
        } else
        {
            RenderAvailableAttacks(ActivePlayer.CurrentUnit);
            RenderDmgFormulas(ActivePlayer.CurrentUnit);
        }
    }

    void UpdateUnitPanel()
    {
        selectedUnitPanel.UpdateUnit(ActivePlayer);
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

    void ClearDamageFormulas()
    {
        dmgFormulas.ForEach(image => Destroy(image));
        dmgFormulas.Clear();
    }

    void RenderAvailableAttacks(Unit unit)
    {
        if (unit == null) return;
        SerializableDictionary<Vector3Int, AttackPatternField> pattern = unit.skillHandler.AttackArea;

        if (pattern != null)
        {
            foreach (KeyValuePair<Vector3Int, AttackPatternField> field in pattern)
            {
                if (field.Value == AttackPatternField.On && TilemapNavigator.Instance.HasTile(field.Key))
                {
                    TintMarker(attackAreaTilemap, field.Key, MarkerTypes.Attack);
                    availableAttacks.Add(field.Key);
                }
            }
        }
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
            {MarkerTypes.Movement, movementTileTint },
            {MarkerTypes.MovementPreview, movementPreviewTileTint }
        };

        Color tintColor = markerColors[type];
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
