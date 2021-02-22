using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum MovementType
{
    Straight,
    Diagonal,
    Both
}

public class Unit : MonoBehaviour
{
    public List<Vector3Int> availableMoves { get; private set; }

    [Header("Movement settings")]
    [SerializeField]
    private MovementType movementType;

    [SerializeField]
    private int straightMovementRange;
    [SerializeField]
    private int diagonalMovementRange;

    [SerializeField]
    private bool canPassUnits;

    [SerializeField]
    private bool canPassObstacles;

    private void Start()
    {
        AlignToGrid();
    }

    public void Focus()
    {
        availableMoves = CalculateAvailableMoves();
    }

    public void Blur()
    {
        availableMoves = null;
    }

    public IEnumerator Move(Vector3Int cellTargetPos)
    {
        Vector3 startingPos = transform.position;
        Vector3 targetPos = TilemapNavigator.Instance.CellToWorldPos(cellTargetPos);

        float movementSpeed = 5;
        float distance = Vector3.Distance(transform.position, targetPos);
        float movementDuration = distance / movementSpeed;

        float elapsedTime = 0f;

        while(elapsedTime < movementDuration)
        {
            transform.position = Vector3.Lerp(startingPos, targetPos, (elapsedTime / movementDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }

    private void AlignToGrid()
    {
        transform.position = new Vector3(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y),
            Mathf.RoundToInt(transform.position.z)
        );
    }

    private List<Vector3Int> CalculateAvailableMoves()
    {
        Dictionary<Vector3Int, int> directions = new Dictionary<Vector3Int, int>();

        if (movementType == MovementType.Straight || movementType == MovementType.Both)
        {
            directions.Add(Vector3Int.up, straightMovementRange);
            directions.Add(Vector3Int.down, straightMovementRange);
            directions.Add(Vector3Int.left, straightMovementRange);
            directions.Add(Vector3Int.right, straightMovementRange);
        }

        if (movementType == MovementType.Diagonal || movementType == MovementType.Both)
        {
            directions.Add(new Vector3Int(-1, -1, 0), diagonalMovementRange);
            directions.Add(new Vector3Int(1, -1, 0), diagonalMovementRange);
            directions.Add(new Vector3Int(-1, 1, 0), diagonalMovementRange);
            directions.Add(new Vector3Int(1, 1, 0), diagonalMovementRange);
        } 

        List<Vector3Int> availableMoves = new List<Vector3Int>();

        foreach(KeyValuePair<Vector3Int, int> direction in directions)
        {
            availableMoves = availableMoves.Union(CheckMovementLine(direction.Key, direction.Value)).ToList();
        }

        return availableMoves;
    }

    List<Vector3Int> CheckMovementLine(Vector3Int direction, int range)
    {
        TilemapNavigator navigator = TilemapNavigator.Instance;
        Vector3Int unitCellPos = navigator.WorldToCellPos(transform.position);

        List<Vector3Int> availableMoves = new List<Vector3Int>();

        for (int i = 1; i <= range; i++)
        {
            Vector3Int nextPosition = unitCellPos + direction * i;

            bool tileExists = navigator.HasTile(nextPosition);
            if (!tileExists) break;

            bool tileIsMoveable = navigator.IsTileMoveable(nextPosition);
            bool tileIsTaken = navigator.IsTileTaken(nextPosition);

            if (tileIsMoveable && !tileIsTaken) availableMoves.Add(nextPosition);

            if (!tileIsMoveable && !canPassObstacles) break;
            if (tileIsTaken && !canPassUnits) break;
        }

        return availableMoves;
    }
}
