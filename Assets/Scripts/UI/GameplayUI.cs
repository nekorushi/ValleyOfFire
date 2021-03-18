using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    public UnityEvent ActivePlayerChanged;

    private Camera mainCamera;
    private RectTransform canvasRect;

    [SerializeField]
    private Image unitTarget;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Sprite availableMovesSprite;
    private List<GameObject> availableMoves = new List<GameObject>();

    [SerializeField]
    private Sprite availableAttacksSprite;
    private List<GameObject> availableAttacks = new List<GameObject>();

    [SerializeField]
    private GameObject dmgFormulaPrefab;
    private List<GameObject> dmgFormulas = new List<GameObject>();

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
        unitTarget.enabled = false;
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
        UpdateUnitTarget();
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

    void UpdateUnitTarget()
    {
        if (activePlayer.CurrentUnit)
        {
            Vector2 canvasPos = WorldToCanvasPos(activePlayer.CurrentUnit.transform.position);

            unitTarget.rectTransform.anchoredPosition = canvasPos;
            unitTarget.enabled = true;
        } else
        {
            unitTarget.enabled = false;
        }
    }

    void ClearAvailableMoves()
    {
        availableMoves.ForEach(image => Destroy(image));
        availableMoves.Clear();
    }

    void RenderAvailableMoves()
    {
        List<Vector3Int> moves = activePlayer.CurrentUnit?.AvailableMoves;
        if (moves != null)
        {
            moves.ForEach(position =>
            {
                GameObject marker = CreateMarker(position, "MovementMarker", availableMovesSprite);
                availableMoves.Add(marker);
            });
        }
    }

    void ClearAvailableAttacks()
    {
        availableAttacks.ForEach(image => Destroy(image));
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

        Skill attackPattern = activePlayer.CurrentUnit.GetAttackPattern(activePlayer.AttackMode);
        SerializableDictionary<Vector3Int, AttackPatternField> pattern = attackPattern.AttackArea;

        if (pattern != null)
        {
            foreach (KeyValuePair<Vector3Int, AttackPatternField> field in pattern)
            {
                if (field.Value == AttackPatternField.On && TilemapNavigator.Instance.HasTile(field.Key))
                {
                    GameObject marker = CreateMarker(field.Key, "AttackMarker", availableAttacksSprite);
                    availableMoves.Add(marker);
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
                        Skill attackPattern = activePlayer.CurrentUnit.GetAttackPattern(activePlayer.AttackMode);
                        DamageValue damage = UnitsConfig.Instance.GetDamageValue(attackPattern.Damage, attacker.UnitType, defender.UnitType);
                        GameObject formula = CreateDmgFormula(defender.CellPosition, damage, defender.UnitType);
                        dmgFormulas.Add(formula);
                    }
                }
            }
        }
    }

    private GameObject CreateMarker(Vector3Int position, string name, Sprite sprite)
    {
        Vector3 worldPos = TilemapNavigator.Instance.CellToWorldPos(position);
        Vector2 canvasPos = WorldToCanvasPos(worldPos);

        GameObject marker = new GameObject(name);
        Image markerSprite = marker.AddComponent<Image>();
        markerSprite.sprite = sprite;
        RectTransform markerRect = marker.GetComponent<RectTransform>();
        markerRect.SetParent(canvasRect);
        markerRect.anchoredPosition = canvasPos;
        markerRect.sizeDelta = Vector2.one;
        return marker;
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
