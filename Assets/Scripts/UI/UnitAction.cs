using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite curveTrajectoryIcon;
    [SerializeField] private Sprite straightTrajectoryIcon;

    [Header("Components references")]
    [SerializeField] private SelectedUnitPanel owner;
    [SerializeField] private Image trajectoryImage;
    [SerializeField] private Image effectImage;
    [SerializeField] private HelpTooltipUserUI trajectoryTooltip;
    [SerializeField] private HelpTooltipUserUI effectTooltip;

    private SkillConfig currentConfig;

    public void SetActions(
        SkillConfig skillConfig
    ) {

        currentConfig = skillConfig;
        RenderInfo(skillConfig);
    }

    private void RenderInfo(SkillConfig skillConfig)
    {
        HideTrajectoryIcon();
        HideEffectIcon();

        if (skillConfig != null)
        {
            RenderTrajectoryIcon(skillConfig.trajectory);
            RenderEffectIcon(skillConfig.effect);
        }
    }

    public Sprite GetTrajectoryIcon(DamageTrajectory trajectory)
    {
        Dictionary<DamageTrajectory, Sprite> trajectoryIcons = new Dictionary<DamageTrajectory, Sprite>()
        {
            { DamageTrajectory.Curve, curveTrajectoryIcon },
            { DamageTrajectory.Straight, straightTrajectoryIcon }
        };

        return trajectoryIcons.ContainsKey(trajectory)
            ? trajectoryIcons[trajectory]
            : null;
    }

    public string GetTrajectoryDescription(DamageTrajectory trajectory)
    {
        Dictionary<DamageTrajectory, string> descriptions = new Dictionary<DamageTrajectory, string>()
        {
            { DamageTrajectory.Curve, "Curve trajectory – projectile is being sent above the ground and can reach any target in range, including targets hidden behind obstacles." },
            { DamageTrajectory.Straight, "Straight trajectory – projectile is being sent in a straight line and can reach only the first target in that line." }
        };

        return descriptions.ContainsKey(trajectory)
            ? descriptions[trajectory]
            : null;
    }

    private void RenderTrajectoryIcon(DamageTrajectory trajectory)
    {
        Sprite trajectoryIcon = GetTrajectoryIcon(trajectory);
        if (trajectoryIcon != null)
        {
            trajectoryImage.sprite = trajectoryIcon;
            trajectoryImage.gameObject.SetActive(true);
            trajectoryTooltip.tooltipText = GetTrajectoryDescription(trajectory);
        }
    }

    private void RenderEffectIcon(AttackEffect effect)
    {
        if (effect.icon != null)
        {
            effectImage.sprite = effect.icon;
            effectImage.gameObject.SetActive(true);
            effectTooltip.tooltipText = string.Format("{0} - {1}", effect.label, effect.description);
        }
    }

    private void HideTrajectoryIcon()
    {
        trajectoryImage.gameObject.SetActive(false);
    }

    private void HideEffectIcon()
    {
        effectImage.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        owner.ShowTooltip(currentConfig, this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        owner.HideTooltip();
    }
}
