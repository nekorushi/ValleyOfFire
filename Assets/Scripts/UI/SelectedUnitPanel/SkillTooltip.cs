using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTooltip : MonoBehaviour
{
    [SerializeField] private Text damageValue;

    [SerializeField] private Image trajectoryIcon;
    [SerializeField] private Text trajectoryLabel;

    [SerializeField] private Image effectIcon;
    [SerializeField] private Text effectLabel;
    [SerializeField] private Text effectDescription;

    public void SetValue(SkillConfig config, UnitAction action)
    {
        Dictionary<DamageTrajectory, string> trajectoryLabels = new Dictionary<DamageTrajectory, string>()
        {
            { DamageTrajectory.Curve, "Curve" },
            { DamageTrajectory.Straight, "Straight" },
            { DamageTrajectory.SelfInflicted, "Self" }
        };

        bool reset = config == null;

        damageValue.text = reset ? "" : config.baseDamage.ToString();

        Sprite trajectorySprite = reset ? null : action.GetTrajectoryIcon(config.trajectory);
        string trajectoryName = reset ? "" : trajectoryLabels[config.trajectory];
        trajectoryLabel.text = trajectoryName;
        trajectoryIcon.sprite = trajectorySprite;
        trajectoryIcon.gameObject.SetActive(trajectorySprite != null);

        effectIcon.sprite = reset ? null : config.effect.icon;
        effectLabel.text = reset ? "" : config.effect.label;
        effectDescription.text = reset ? "" : config.effect.description;
    }
}
