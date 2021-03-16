using UnityEngine;

public abstract class UnitStatus : ScriptableObject
{
    public abstract void OnAdd(Unit afflictedUnit);
    public abstract bool OnTick(Unit afflictedUnit);
}
