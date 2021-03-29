using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveUnitUI : MonoBehaviour
{
    [SerializeField]
    List<Image> actionPoints;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Image portrait;

    [SerializeField]
    private Sprite emptyPortrait;

    [SerializeField]
    private Sprite pointOn;

    [SerializeField]
    private Sprite pointOff;


    private void UpdatePortrait(PlayerController player, Unit unit)
    {
        portrait.sprite = unit.unitClass.portraits[player.faction];
    }
    private void UpdateValue(int value)
    {
        ClearValue();

        for (int i = 0; i < value; i++)
        {
            actionPoints[i].sprite = pointOn;
        }
    }

    private void UpdateEnabled(bool enable)
    {
        Color newColor = background.color;
        newColor.a = enable ? 1f : .25f;

        background.color = newColor;
    }

    private void ClearPortrait()
    {
        portrait.sprite = emptyPortrait;
    }

    private void ClearValue()
    {
        foreach (Image point in actionPoints)
        {
            point.sprite = pointOff;
        }
    }

    public void SetUnit(
        PlayerController player,
        Unit unit,
        int actionPoints,
        bool enabled
    ) {
        UpdatePortrait(player, unit);
        UpdateValue(actionPoints);
        UpdateEnabled(enabled);
    }

    public void ClearUnit()
    {
        ClearPortrait();
        ClearValue();
        UpdateEnabled(false);
    }
}
