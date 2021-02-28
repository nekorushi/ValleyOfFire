using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

enum MovementType
{
    Straight,
    Diagonal,
    Both
}

[RequireComponent(typeof(AttackPattern))]
public class Unit : MonoBehaviour
{
    private PlayerController owner;
    [SerializeField]
    private TMP_Text healthText;
    [SerializeField]
    private TMP_Text damageText;

    private readonly float UNIT_Z_POSITION = -0.5f;
    public List<Vector3Int> availableMoves { get; private set; }
    public Vector3Int CellPosition { get { return TilemapNavigator.Instance.WorldToCellPos(transform.position); } }

    [Header("Unit settings")]
    [SerializeField]
    private UnitTypes unitType;
    [SerializeField]
    private float _health = 5f;
    public float Health { get { return _health; } private set { _health = value; } }

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
    public AttackPattern AttackPattern { get; private set; }

    public void SetOwner(PlayerController player)
    {
        owner = player;

        SpriteRenderer unitSprite = GetComponentInChildren<SpriteRenderer>();
        if (unitSprite) unitSprite.color = player.PlayerColor;
    }

    private void Start()
    {
        AttackPattern = GetComponent<AttackPattern>();
        AlignToGrid();
        UpdateHealthText();
    }

    public void Focus()
    {
        availableMoves = CalculateAvailableMoves();
    }

    public void Blur()
    {
        availableMoves = null;
    }

    public void ApplyDamage(float amount)
    {
        Health = Mathf.Clamp(Health - amount, 0, Health);
        UpdateHealthText();
        StartCoroutine(AnimateDamage(amount));

        if (Health == 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdateHealthText()
    {
        healthText.text = Health.ToString("N1");
    }

    private IEnumerator AnimateDamage(float amount)
    {
        damageText.text = amount.ToString();
        damageText.gameObject.SetActive(true);

        for (float current = 0; current < 1; current += 0.1f)
        {
            damageText.rectTransform.anchoredPosition = new Vector2(damageText.rectTransform.anchoredPosition.x, current);
            yield return null;
        }

        damageText.gameObject.SetActive(false);
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

    public bool Attack(Vector3Int clickedPos, Unit clickedUnit)
    {
        Vector3Int clickRelativePos = clickedPos - CellPosition;
        Vector2Int clickRelativePos2D = new Vector2Int(clickRelativePos.x, clickRelativePos.y);
        SerializableDictionary<Vector2Int, AttackPatternField> fields = AttackPattern.Pattern;
        bool isAttackClicked = fields.ContainsKey(clickRelativePos2D) && fields[clickRelativePos2D] == AttackPatternField.On;
        if (isAttackClicked)
        {
            switch (AttackPattern.attackType)
            {
                case AttackType.Targeted:
                    if (clickedUnit != null)
                    {
                        UnitTypes defenderType = clickedUnit.unitType;
                        clickedUnit.ApplyDamage(UnitsConfig.Instance.GetDamageValue(unitType, defenderType));
                        return true;
                    }
                    break;
                case AttackType.Area:
                    PerformAreaAttack();
                    return true;
            }
        }

        return false;
    }

    private void PerformAreaAttack()
    {
        SerializableDictionary<Vector2Int, AttackPatternField> fields = AttackPattern.Pattern;

        foreach (KeyValuePair<Vector2Int, AttackPatternField> field in fields)
        {
            if (field.Value == AttackPatternField.On)
            {
                Unit reachedUnit = TilemapNavigator.Instance.GetUnit(CellPosition + new Vector3Int(field.Key.x, field.Key.y, 0));
                if (reachedUnit && !owner.Units.Contains(reachedUnit))
                {
                    UnitTypes defenderType = reachedUnit.unitType;
                    reachedUnit.ApplyDamage(UnitsConfig.Instance.GetDamageValue(unitType, defenderType));
                }
            }
        }
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
