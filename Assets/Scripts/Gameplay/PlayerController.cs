using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Component configuration")]
    public UnityEvent UnitSelectionChanged;
    private Camera mainCamera;

    [SerializeField]
    private LayerMask layerMask;

    [Header("Gameplay settings")]
    private byte currentActionPoints = 0;

    [SerializeField]
    private string _playerName;
    public string PlayerName { get { return _playerName; } }

    [SerializeField]
    private List<Unit> units;

    [SerializeField]
    private byte maxActionPoints = 2;

    private Unit _currentUnit;
    public Unit currentUnit {
        get { return _currentUnit; }
        private set
        {
            _currentUnit?.Blur();
            _currentUnit = value;
            _currentUnit?.Focus();
            UnitSelectionChanged.Invoke();
        } 
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    public IEnumerator PerformTurn()
    {
        currentActionPoints = maxActionPoints;

        while(currentActionPoints > 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                yield return StartCoroutine(HandleMouseClick());
            }

            yield return null;
        }
        Reset();
    }

    private void Reset()
    {
        SelectUnit(null);
    }

    private IEnumerator HandleMouseClick()
    {
        Vector3 clickedWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(clickedWorldPos, Vector2.zero, Mathf.Infinity, layerMask);

        if (hit.collider != null)
        {
            Unit clickedUnit = hit.collider.gameObject.GetComponent<Unit>();

            if (clickedUnit) {
                bool shouldUnselect = currentUnit == clickedUnit || units.Contains(clickedUnit);
                SelectUnit(shouldUnselect ? null : clickedUnit);
            } else
            {
                Vector3Int clickedCellPos = TilemapNavigator.Instance.WorldToCellPos(clickedWorldPos);
                if (currentUnit) yield return StartCoroutine(PerformUnitAction(clickedCellPos));
            }
        }
    }

    private void SelectUnit(Unit unit)
    {
        currentUnit = unit;
    }

    private IEnumerator PerformUnitAction(Vector3Int clickedPos)
    {
        bool isMovementClicked = currentUnit.availableMoves.Contains(clickedPos);
        if (isMovementClicked)
        {
            Unit actingUnit = currentUnit;
            SelectUnit(null);
            yield return StartCoroutine(actingUnit.Move(clickedPos));
            SelectUnit(actingUnit);
        }

        currentActionPoints -= 1;
    }
}
