using UnityEngine;
using UnityEngine.EventSystems;

public class HelpTooltipUserUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tooltipText;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        HelpTooltip.Instance.Show(corners[3], tooltipText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HelpTooltip.Instance.Hide();
    }
}
