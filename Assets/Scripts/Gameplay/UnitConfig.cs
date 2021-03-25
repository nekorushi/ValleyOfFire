using System;
using UnityEngine;

[Serializable] public class FactionSpriteDict : SerializableDictionary<PlayerFaction, Sprite> { }

[CreateAssetMenu(fileName = "UnitConfig", menuName = "GDS/Units/UnitConfig", order = 1)]
public class UnitConfig : ScriptableObject
{
    [SerializeField]
    private UnitTypes _type;
    public UnitTypes Type { get { return _type; } }

    [SerializeField]
    public FactionSpriteDict portraits = new FactionSpriteDict();
    [SerializeField]
    public FactionSpriteDict inGameSprites = new FactionSpriteDict();

    [SerializeField]
    private float _baseHealth = 5f;
    public float BaseHealth { get { return _baseHealth; } }

    [SerializeField]
    private int _movementRange;
    public int MovementRange { get { return _movementRange; } }

    [SerializeField]
    private int _swampedMovementRange;
    public int SwampedMovementRange { get { return _swampedMovementRange; } }

    public SkillConfig primarySkill;
    public SkillConfig secondarySkill;
}
