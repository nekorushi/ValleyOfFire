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

[RequireComponent(typeof(AttackPattern))]
public class Unit : MonoBehaviour
{
    private readonly float UNIT_Z_POSITION = -0.5f;
    public List<Vector3Int> availableMoves { get; private set; }
    public Vector3Int CellPosition { get { return TilemapNavigator.Instance.WorldToCellPos(transform.position); } }

    [Header("Unit settings")]
    [SerializeField]
    private int _health = 5;
    public int Health { get { return _health; } private set { _health = value; } }

    [SerializeField]
    private int _attackDmg = 5;
    public int AttackDmg { get { return _attackDmg; } private set { _attackDmg = value; } }

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

    private AttackPattern _attackPattern;
    public AttackPattern attackPattern { get { return _attackPattern; } }

    private void Start()
    {
        _attackPattern = GetComponent<AttackPattern>();
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

    public void ApplyDamage(int amount)
    {
        Health = Mathf.Clamp(Health - amount, 0, Health);

        if (Health == 0)
        {
            gameObject.SetActive(false);
        }
    }

    public IEnumerator Move(Vector3Int cellTargetPos)
    {
        Vector3 startingPos = transform.position;
        Vector3 targetWorldPos = TilemapNavigator.Instance.CellToWorldPos(cellTargetPos);
        Vector3 targetPos = new Vector3(targetWorldPos.x, targetWorldPos.y, UNIT_Z_POSITION);

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
            UNIT_Z_POSITION
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
        List<Vector3Int> availableMoves = new List<Vector3Int>();

        for (int i = 1; i <= range; i++)
        {
            TilemapNavigator navigator = TilemapNavigator.Instance;
            Vector3Int nextPosition = CellPosition + direction * i;

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
