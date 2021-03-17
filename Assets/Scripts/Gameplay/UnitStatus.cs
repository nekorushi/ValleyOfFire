﻿using UnityEngine;

public abstract class UnitStatus : ScriptableObject
{
    public string StatusName
    {
        get { return this.GetType().ToString(); }
    }

    public abstract void OnAdd(Unit afflictedUnit);
    public abstract bool OnTick(Unit afflictedUnit);
}
