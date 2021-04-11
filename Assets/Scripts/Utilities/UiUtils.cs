using UnityEngine;

public static class UiUtils
{
    public static bool[] CheckCornersVisibilityFrom(this RectTransform rectTransform, Camera camera)
    {
        Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);

        bool[] cornersVisibility = new bool[4];
        Vector3 tempScreenSpaceCorner; // Cached
        for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
        {
            tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
            cornersVisibility[i] = screenBounds.Contains(tempScreenSpaceCorner);
        }

        return cornersVisibility;
    }
}