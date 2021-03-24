using UnityEngine;
using UnityEngine.UI;

public class StatusIcon : MonoBehaviour
{
    [SerializeField]
    private GameObject iconWrapper;

    [SerializeField]
    private Image icon;

    void Awake()
    {
        iconWrapper.SetActive(false);
    }

    public void SetValue(UnitStatus status)
    {
        if (status != null)
        {
            iconWrapper.SetActive(true);
            icon.sprite = status.icon;
            icon.color = status.iconColor;
        } else
        {
            iconWrapper.SetActive(false);
        }
    }
}
