using UnityEngine;
using UnityEngine.UI;

public class SegmentedBar : MonoBehaviour, IValueBar
{
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private GameObject barWrapper;

    [SerializeField] private Color segmentColor;

    public void SetValue(float currentValue, float maxValue)
    {
        int currentSegments = Mathf.CeilToInt(currentValue);
        int maxSegments = Mathf.CeilToInt(maxValue);

        foreach (Transform segment in barWrapper.transform)
        {
            Destroy(segment.gameObject);
        }

        for (int i = 1; i <= maxSegments; i++)
        {
            GameObject newSegment = Instantiate(segmentPrefab);
            Image segmentImage = newSegment.GetComponent<Image>();

            Color newSegmentColor = segmentColor;
            if (i > currentSegments) newSegmentColor.a = 0;

            segmentImage.color = newSegmentColor;

            newSegment.transform.SetParent(barWrapper.transform);
        }
    }
}
