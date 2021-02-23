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
    private Text currentPlayerText;

    [SerializeField]
    private Sprite availableMovesSprite;
    private List<GameObject> availableMoves = new List<GameObject>();

    private PlayerController _activePlayer;
    public PlayerController activePlayer {
        get { return _activePlayer; }
        set
        {
            DisconnectPlayer();
            _activePlayer = value;
            ConnectPlayer();
            ActivePlayerChanged.Invoke();
            Debug.Log("Active player changed");
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        canvasRect = GetComponent<RectTransform>();
        unitTarget.enabled = false;
    }

    void DisconnectPlayer()
    {
        activePlayer?.UnitSelectionChanged.RemoveListener(UpdateUnitSelection);
    }

    void ConnectPlayer()
    {
        activePlayer?.UnitSelectionChanged.AddListener(UpdateUnitSelection);
        UpdatePlayerText();
    }

    void UpdatePlayerText()
    {
        if (activePlayer) currentPlayerText.text = activePlayer.PlayerName;
    }

    void UpdateUnitSelection()
    {
        UpdateUnitTarget();
        UpdateAvailableMoves();
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

    void UpdateAvailableMoves()
    {
        availableMoves.ForEach(image => Destroy(image));
        availableMoves.Clear();

        List<Vector3Int> moves = activePlayer.currentUnit?.availableMoves;
        if (moves != null) 
        {
            moves.ForEach(position =>
            {
                Vector3 worldPos = TilemapNavigator.Instance.CellToWorldPos(position);
                Vector2 canvasPos = WorldToCanvasPos(worldPos);

                GameObject marker = new GameObject("MovementMarker");
                Image markerSprite = marker.AddComponent<Image>();
                markerSprite.sprite = availableMovesSprite;
                RectTransform markerRect = marker.GetComponent<RectTransform>();
                markerRect.SetParent(canvasRect);
                markerRect.anchoredPosition = canvasPos;
                markerRect.sizeDelta = Vector2.one;
                availableMoves.Add(marker);
            });
        }
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
