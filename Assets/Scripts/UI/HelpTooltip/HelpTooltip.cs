using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class HelpTooltip : MonoBehaviour
{
    private static HelpTooltip _instance;
    public static HelpTooltip Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<HelpTooltip>();
            return _instance;
        }
    }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private RectTransform rectTransform;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        Hide();
    }

    public void Show(Vector3 screenPosition, string text)
    {
        transform.position = screenPosition;
        tooltipText.text = text;

        SetAnchor(false, false);


        bool[] cornersVisibility = rectTransform.CheckCornersVisibilityFrom(mainCamera);
        if (cornersVisibility.ToList().Count(isVisible => !isVisible) > 0)
        {
            SetAnchor(!cornersVisibility[2] && !cornersVisibility[3], !cornersVisibility[0] && !cornersVisibility[3]);
        }

        panel.SetActive(text != null && text != "");
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    public void SetAnchor(bool moveToLeft, bool moveToTop)
    {
        rectTransform.pivot = new Vector2(moveToLeft ? 1 : 0, moveToTop ? 0 : 1);
    }
}
