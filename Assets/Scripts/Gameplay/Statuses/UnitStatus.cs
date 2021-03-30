using System;
using UnityEngine;

[Serializable] public class ShieldRecutionDict : SerializableDictionary<UnitType, int> { }

public abstract class UnitStatus : ScriptableObject
{
    public Sprite icon;
    public Color iconColor;

    [SerializeField]
    private ShieldRecutionDict shieldReduction = new ShieldRecutionDict();

    public string StatusName
    {
        get { return this.GetType().ToString(); }
    }

    public int GetShieldReduction(UnitType type)
    {
        return shieldReduction.ContainsKey(type) ? shieldReduction[type] : 0;
    }

    public abstract void OnAdd(Unit afflictedUnit);
    public abstract bool OnTick(Unit afflictedUnit);
    public abstract void OnRemove(Unit afflictedUnit);
}
