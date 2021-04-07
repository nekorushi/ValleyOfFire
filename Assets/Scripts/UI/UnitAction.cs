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

    private void RenderTrajectoryIcon(DamageTrajectory trajectory)
    {
        Sprite trajectoryIcon = GetTrajectoryIcon(trajectory);
        if (trajectoryIcon != null)
        {
            trajectoryImage.sprite = trajectoryIcon;
            trajectoryImage.gameObject.SetActive(true);
        }
    }

    private void RenderEffectIcon(AttackEffect effect)
    {
        if (effect.icon != null)
        {
            effectImage.sprite = effect.icon;
            effectImage.gameObject.SetActive(true);
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
