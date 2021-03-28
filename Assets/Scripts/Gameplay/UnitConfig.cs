using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class FactionSpriteDict : SerializableDictionary<PlayerFaction, Sprite> { }
[Serializable] public class ClassSpriteDict : SerializableDictionary<UnitTypes, Sprite> { }

[CreateAssetMenu(fileName = "UnitConfig", menuName = "GDS/Units/UnitConfig")]
public class UnitConfig : ScriptableObject
{
    [Header("Unit display settings")]
    public Sprite icon;
    public FactionSpriteDict portraits = new FactionSpriteDict();
    public FactionSpriteDict inGameSprites = new FactionSpriteDict();

    [Header("Unit stats"), Space(20)]
    [SerializeField]
    private UnitTypes _type;
    public UnitTypes Type { get { return _type; } }

    [SerializeField]
    private float _baseHealth = 5f;
    public float BaseHealth { get { return _baseHealth; } }

    [SerializeField]
    private int _movementRange;
    public int MovementRange { get { return _movementRange; } }

    [SerializeField]
    private int _swampedMovementRange;
    public int SwampedMovementRange { get { return _swampedMovementRange; } }

    [Header("Resistances"), Space(20)]
    public List<Resistance> resistances;

    [Header("Skills settings"), Space(20)]
    public SkillConfig primarySkill;
    public SkillConfig secondarySkill;
}
