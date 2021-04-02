using UnityEngine;
using UnityEngine.UI;

public class ActiveUnitUI : MonoBehaviour
{
    [SerializeField]
    Image movePoint;

    [SerializeField]
    Image actionPoint;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Image portrait;

    [SerializeField]
    private Sprite emptyPortrait;

    [SerializeField]
    private Color pointOn;

    [SerializeField]
    private Color pointOff;


    private void UpdatePortrait(PlayerController player, Unit unit)
    {
        portrait.sprite = unit.unitClass.portraits[player.faction];
    }
    private void UpdateValue(ActionPoints value)
    {
        ClearValue();

        movePoint.color = value.canMove ? pointOff : pointOn;
        actionPoint.color = value.canAttack ? pointOff : pointOn;
    }

    private void UpdateEnabled(bool enable)
    {
        Color newBackgroundColor = background.color;
        Color newPortraitColor = portrait.color;
        Color newMovePointColor = movePoint.color;
        Color newActionPointColor = actionPoint.color;

        float newOpacity = enable ? 1f : .25f;

        newBackgroundColor.a = newOpacity;
        newPortraitColor.a = newOpacity;
        newMovePointColor.a = newOpacity;
        newActionPointColor.a = newOpacity;

        portrait.color = newPortraitColor;
        background.color = newBackgroundColor;
        movePoint.color = newMovePointColor;
        actionPoint.color = newActionPointColor;
    }

    private void ClearPortrait()
    {
        portrait.sprite = emptyPortrait;
    }

    private void ClearValue()
    {
        movePoint.color = pointOff;
        actionPoint.color = pointOff;
    }

    public void SetUnit(
        PlayerController player,
        Unit unit,
        ActionPoints actionPoints,
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
