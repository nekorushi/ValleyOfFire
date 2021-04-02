using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitAction : MonoBehaviour
{
    [SerializeField]
    private Sprite curveTrajectoryIcon;

    [SerializeField]
    private Sprite straightTrajectoryIcon;

    [Header("Elements references")]
    [SerializeField]
    private Image trajectoryImage;

    [SerializeField]
    private Image effectImage;

    public void SetActions(
        SkillConfig skillConfig
    ) {

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

    private void RenderTrajectoryIcon(DamageTrajectory trajectory)
    {

        Dictionary<DamageTrajectory, Sprite> trajectoryIcons = new Dictionary<DamageTrajectory, Sprite>()
        {
            { DamageTrajectory.Curve, curveTrajectoryIcon },
            { DamageTrajectory.Straight, straightTrajectoryIcon }
        };

        if (trajectoryIcons.ContainsKey(trajectory))
        {
            trajectoryImage.sprite = trajectoryIcons[trajectory];
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
}
