using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

    private void Start()
    {
        mainCamera = Camera.main;
        canvasRect = GetComponent<RectTransform>();
        unitTarget.enabled = false;
    }

    void ConnectPlayer()
    {
        activePlayer?.UnitSelectionChanged.AddListener(UpdateUnitSelection);
        activePlayer?.ControlModeChanged.AddListener(UpdateAvailableActions);
        activePlayer?.AvailableActionsChanged.AddListener(UpdateAvailableActions);
    }

    void DisconnectPlayer()
    {
        activePlayer?.UnitSelectionChanged.RemoveListener(UpdateUnitSelection);
        activePlayer?.ControlModeChanged.RemoveListener(UpdateAvailableActions);
        activePlayer?.AvailableActionsChanged.RemoveListener(UpdateAvailableActions);
    }

    void UpdateUnitSelection()
    {
        UpdateUnitTarget();
        UpdateAvailableActions();
    }

    void UpdateAvailableActions()
    {
        ClearAvailableMoves();

        if (activePlayer.ControlMode == ControlModes.Movement)
        {
            RenderAvailableMoves();
        } else
        {
            RenderAvailableAttacks();
        }
    }

    void UpdateUnitTarget()
    {
        if (activePlayer.currentUnit)
        {
            Vector2 canvasPos = WorldToCanvasPos(activePlayer.currentUnit.transform.position);

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
        ClearAvailableMoves();

        List<Vector3Int> moves = activePlayer.currentUnit?.availableMoves;
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

    void RenderAvailableAttacks()
    {

        ClearAvailableAttacks();

        SerializableDictionary<Vector2Int, AttackPatternField> pattern
            = activePlayer.currentUnit?.AttackPattern.Pattern;

        if (pattern != null)
        {
            foreach (KeyValuePair<Vector2Int, AttackPatternField> field in pattern)
            {
                Vector3Int position = activePlayer.currentUnit.CellPosition + new Vector3Int(field.Key.x, field.Key.y, 0);

                if (field.Value == AttackPatternField.On && TilemapNavigator.Instance.HasTile(position))
                {
                    GameObject marker = CreateMarker(position, "AttackMarker", availableAttacksSprite);
                    availableMoves.Add(marker);
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

    private Vector2 WorldToCanvasPos(Vector3 worldPos)
    {
        Vector2 viewportPos = mainCamera.WorldToViewportPoint(worldPos);
        return new Vector2(
            viewportPos.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f,
            viewportPos.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f
        );
    }
}
