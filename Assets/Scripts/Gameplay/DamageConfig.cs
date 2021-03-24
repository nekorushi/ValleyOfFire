using System.Collections.Generic;
using UnityEngine;

public class DamageConfig
{
    public enum Types
    {
        Heal,
        Fire,
        Normal
    }

    public static Dictionary<Types, Color> Colors = new Dictionary<Types, Color>() {
        { Types.Normal, Color.red },
        { Types.Fire, new Color(1, .5f, 0) },
        { Types.Heal, Color.green }
    };
}
