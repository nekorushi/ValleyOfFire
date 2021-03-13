using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Pathfinding;

enum MovementType
{
    Straight,
    Diagonal,
    Both
}

[RequireComponent(typeof(AttackPattern))]
public class Unit : MonoBehaviour
{
    public PlayerController Owner { get; private set; }
    [SerializeField]
    private TMP_Text healthText;
    [SerializeField]
    private TMP_Text damageText;

    private readonly float UNIT_Z_POSITION = -0.5f;
    public List<Vector3Int> AvailableMoves { get; private set; }
    public Vector3Int CellPosition { get { return TilemapNavigator.Instance.WorldToCellPos(transform.position); } }

    [Header("Unit settings")]
    [SerializeField]
    private UnitTypes _unitType;
    public UnitTypes UnitType { get { return _unitType; } private set { _unitType = value; } }

    [SerializeField]
    private float _health = 5f;
    public float Health { get { return _health; } private set { _health = value; } }

    [SerializeField]
    private int movementRange;
    private TilesetTraversalProvider tilesetTraversalProvider;

    public AttackPattern PrimaryAttack { get; private set; }
    public AttackPattern SecondaryAttack { get; private set; }

    public void SetOwner(PlayerController player)
    {
        Owner = player;

        SpriteRenderer unitSprite = GetComponentInChildren<SpriteRenderer>();
        if (unitSprite) unitSprite.color = player.PlayerColor;
    }

    private void Start()
    {
        AttackPattern[] attackPatterns = GetComponents<AttackPattern>();

        PrimaryAttack = attackPatterns[0];
        SecondaryAttack = attackPatterns[1];

        AlignToGrid();
        UpdateHealthText();

        tilesetTraversalProvider = UnitBlockManager.Instance.traversalProvider;
        tilesetTraversalProvider.ReserveNode(CellPosition);
    }

    public void Focus() {
        UnitBlockManager.Instance.traversalProvider.selectedUnitCell = CellPosition;
        UpdateAvailableMoves();
    }

    public void Blur()
    {
        AvailableMoves = null;
    }

    public AttackPattern GetAttackPattern(AttackModes mode)
    {
        if (mode == AttackModes.Primary) return PrimaryAttack;
        return SecondaryAttack;
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

        for (float current = 0; current < 1f; current += 0.1f)
        {
            damageText.rectTransform.anchoredPosition = new Vector2(damageText.rectTransform.anchoredPosition.x, current);
            yield return new WaitForSeconds(.03f);
        }

        damageText.gameObject.SetActive(false);
    }

    public IEnumerator Move(Vector3Int cellTargetPos)
    {
        TilemapNavigator navigator = TilemapNavigator.Instance;

        Vector3 startingPos = transform.position;
        Vector3 targetWorldPos = navigator.CellToWorldPos(cellTargetPos);
        Vector3 targetPos = new Vector3(targetWorldPos.x, targetWorldPos.y, UNIT_Z_POSITION);

        float movementSpeed = 3;
        float movementDuration = 1 / movementSpeed;

        Path path = ABPath.Construct(CellPosition, cellTargetPos, null);
        path.traversalProvider = tilesetTraversalProvider;
        AstarPath.StartPath(path);


        yield return StartCoroutine(path.WaitForPath());
        if (!path.error)
        {
            List<Vector3> waypoints = path.vectorPath.ConvertAll(node => navigator.CellToWorldPos(navigator.WorldToCellPos(node)));

            for (int currentTarget = 1; currentTarget < waypoints.Count; currentTarget++)
            {
                float elapsedTime = 0f;
                Vector3 currentStart = currentTarget == 1 ? startingPos : waypoints[currentTarget - 1];
                while (elapsedTime < movementDuration)
                {
                    transform.position = Vector3.Lerp(currentStart, waypoints[currentTarget], (elapsedTime / movementDuration));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            tilesetTraversalProvider.ReleaseNode(CellPosition);
            tilesetTraversalProvider.ReserveNode(cellTargetPos);
            transform.position = targetPos;
        }
    }

    public bool Attack(Vector3Int clickedPos, Unit clickedUnit)
    {
        Vector3Int clickRelativePos = clickedPos - CellPosition;
        Vector2Int clickRelativePos2D = new Vector2Int(clickRelativePos.x, clickRelativePos.y);
        AttackPattern attackPattern = GetAttackPattern(Owner.AttackMode);
        SerializableDictionary<Vector2Int, AttackPatternField> fields = attackPattern.Pattern;

        bool isAttackClicked = fields.ContainsKey(clickRelativePos2D) && fields[clickRelativePos2D] == AttackPatternField.On;
        if (isAttackClicked)
        {
            switch (attackPattern.attackType)
            {
                case AttackType.Targeted:
                    if (clickedUnit != null)
                    {
                        UnitTypes defenderType = clickedUnit.UnitType;
                        DamageValue damageInflicted = UnitsConfig.Instance.GetDamageValue(attackPattern.Damage, UnitType, defenderType);
                        clickedUnit.ApplyDamage(damageInflicted.totalDamage);
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
        AttackPattern attackPattern = GetAttackPattern(Owner.AttackMode);
        SerializableDictionary<Vector2Int, AttackPatternField> fields = attackPattern.Pattern;

        foreach (KeyValuePair<Vector2Int, AttackPatternField> field in fields)
        {
            if (field.Value == AttackPatternField.On)
            {
                Unit reachedUnit = TilemapNavigator.Instance.GetUnit(CellPosition + new Vector3Int(field.Key.x, field.Key.y, 0));
                if (reachedUnit && !Owner.Units.Contains(reachedUnit))
                {
                    UnitTypes defenderType = reachedUnit.UnitType;
                    DamageValue damageInflicted = UnitsConfig.Instance.GetDamageValue(attackPattern.Damage, UnitType, defenderType);
                    reachedUnit.ApplyDamage(damageInflicted.totalDamage);
                }
            }
        }
    }

    private void AlignToGrid()
    {
        TilemapNavigator navigator = TilemapNavigator.Instance;
        Vector3 alignedPosition = navigator.CellToWorldPos(navigator.WorldToCellPos(transform.position));

        transform.position = new Vector3(alignedPosition.x, alignedPosition.y, UNIT_Z_POSITION);
    }

    public void UpdateAvailableMoves()
    {
        AvailableMoves = TilemapNavigator.Instance.CalculateMovementRange(CellPosition, movementRange);
    }
}
