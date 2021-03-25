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
    Attack,
}

public class GameplayUI : MonoBehaviour
{
    public UnityEvent ActivePlayerChanged;

    private Camera mainCamera;
    private RectTransform canvasRect;

    [SerializeField]
    private Tilemap areaTilemap;

    [SerializeField]
    private List<Vector3Int> availableMoves = new List<Vector3Int>();

    [SerializeField]
    private List<Vector3Int> availableAttacks = new List<Vector3Int>();

    [SerializeField]
    private Color defaultTileTint = Color.white;
    [SerializeField]
    private Color movementTileTint = Color.green;
    [SerializeField]
    private Color attackTileTint = Color.red;

    [SerializeField]
    private GameObject dmgFormulaPrefab;
    private List<GameObject> dmgFormulas = new List<GameObject>();

    [SerializeField]
    private SelectedUnitPanel selectedUnitPanel;

    [SerializeField]
    private List<PlayerController> allPlayers;

    private PlayerController _activePlayer;
    public PlayerController activePlayer {
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
        ResetTint();
    }

    void ConnectPlayer()
    {
        if (activePlayer != null)
        {
            activePlayer.UnitSelectionChanged.AddListener(UpdateUnitSelection);
            activePlayer.ControlModeChanged.AddListener(UpdateAvailableActions);
            activePlayer.AvailableActionsChanged.AddListener(UpdateAvailableActions);
        }
    }

    void DisconnectPlayer()
    {
        if (activePlayer != null)
        {
            activePlayer.UnitSelectionChanged.RemoveListener(UpdateUnitSelection);
            activePlayer.ControlModeChanged.RemoveListener(UpdateAvailableActions);
            activePlayer.AvailableActionsChanged.RemoveListener(UpdateAvailableActions);
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

        if (activePlayer.AttackMode == AttackModes.None)
        {
            RenderAvailableMoves();
        } else
        {
            RenderAvailableAttacks();
            RenderDmgFormulas();
        }
    }

    void UpdateUnitPanel()
    {
        selectedUnitPanel.UpdateUnit(activePlayer);
    }

    void ClearAvailableMoves()
    {
        availableMoves.ForEach(position => TintMarker(position, MarkerTypes.Default));
        availableMoves.Clear();
    }

    void RenderAvailableMoves()
    {
        List<Vector3Int> moves = activePlayer.CurrentUnit?.AvailableMoves;
        if (moves != null)
        {
            moves.ForEach(position =>
            {
                TintMarker(position, MarkerTypes.Movement);
                availableMoves.Add(position);
            });
        }
    }

    void ClearAvailableAttacks()
    {
        availableAttacks.ForEach(position => TintMarker(position, MarkerTypes.Default));
        availableAttacks.Clear();
    }

    void ClearDamageFormulas()
    {
        dmgFormulas.ForEach(image => Destroy(image));
        dmgFormulas.Clear();
    }

    void RenderAvailableAttacks()
    {
        if (activePlayer.CurrentUnit == null) return;
        SerializableDictionary<Vector3Int, AttackPatternField> pattern = activePlayer.CurrentUnit.skillHandler.AttackArea;

        if (pattern != null)
        {
            foreach (KeyValuePair<Vector3Int, AttackPatternField> field in pattern)
            {
                if (field.Value == AttackPatternField.On && TilemapNavigator.Instance.HasTile(field.Key))
                {
                    TintMarker(field.Key, MarkerTypes.Attack);
                    availableMoves.Add(field.Key);
                }
            }
        }
    }

    void RenderDmgFormulas()
    {
        if (activePlayer.CurrentUnit == null) return;

        foreach(PlayerController player in allPlayers)
        {
            if (player != activePlayer)
            {
                foreach(Unit defender in player.Units)
                {
                    Unit attacker = activePlayer.CurrentUnit;
                    if (defender != null && defender.Owner != activePlayer && defender.Health > 0)
                    {
                        SkillConfig skillConfig = activePlayer.CurrentUnit.GetSkillConfig(activePlayer.AttackMode);
                        DamageValue damage = UnitsConfig.Instance.GetDamageValue(skillConfig.damage, attacker.unitClass.Type, defender.unitClass.Type);
                        GameObject formula = CreateDmgFormula(defender.CellPosition, damage, defender.unitClass.Type);
                        dmgFormulas.Add(formula);
                    }
                }
            }
        }
    }

    private void TintMarker(Vector3Int position, MarkerTypes type)
    {
        Dictionary<MarkerTypes, Color> markerColors = new Dictionary<MarkerTypes, Color>()
        {
            {MarkerTypes.Default, defaultTileTint },
            {MarkerTypes.Attack, attackTileTint },
            {MarkerTypes.Movement, movementTileTint }
        };

        Color tintColor = markerColors[type];
        areaTilemap.SetColor(position, tintColor);
    }

    private void ResetTint()
    {
        BoundsInt bounds = areaTilemap.cellBounds;

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                for (int z = bounds.min.z; z < bounds.max.z; z++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, z);
                    if (areaTilemap.HasTile(cellPos))
                    {
                        TintMarker(cellPos, MarkerTypes.Default);
                    }
                }
            }

        }
    }

    private GameObject CreateDmgFormula(Vector3Int position, DamageValue damage, UnitTypes defenderClass)
    {
        Vector3 worldPos = TilemapNavigator.Instance.CellToWorldPos(position);
        Vector2 canvasPos = WorldToCanvasPos(worldPos);

        GameObject formula = Instantiate(dmgFormulaPrefab);
        TMP_Text value = formula.GetComponentInChildren<TMP_Text>();

        string messageFormat = damage.bonusDamage == 0f
            ? "{0}"
            : damage.bonusDamage > 0 ? "{0}\n(+{1})" : "{0}\n({1})";

        value.text = string.Format(messageFormat,  damage.baseDamage, damage.bonusDamage, defenderClass);

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
