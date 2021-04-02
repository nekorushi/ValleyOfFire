public static class UIHelpers
{
    public static string FormatHealth(float health, bool round = false)
    {
        return round ? health.ToString() : health.ToString("N1");
    }

    public static string FormatShield(float shield)
    {
        return shield.ToString();
    }
}
