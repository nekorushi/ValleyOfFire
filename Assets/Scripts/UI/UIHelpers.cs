public static class UIHelpers
{
    public static string FormatHealth(float health)
    {
        return health.ToString("N1");
    }

    public static string FormatShield(float shield)
    {
        return shield.ToString();
    }
}
