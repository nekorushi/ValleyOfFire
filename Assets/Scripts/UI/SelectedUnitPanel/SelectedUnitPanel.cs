using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedUnitPanel : MonoBehaviour
{
    private PlayerController currentPlayer;

    [SerializeField] private GameObject wrapper;
    [SerializeField] private List<Image> teamColors;
    [SerializeField] private Image portrait;

    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text shieldText;

    [SerializeField] private Button baseAttack;
    [SerializeField] private Button mainAbility;
    [SerializeField] private Button secondaryAbility;

    [SerializeField] private UnitAction baseAttackPanel;
    [SerializeField] private UnitAction mainAbilityPanel;
    [SerializeField] private UnitAction secondaryAbilityPanel;

    [SerializeField] private SkillTooltip skillTooltip;

    private void Awake()
    {
        baseAttack.onClick.AddListener(OnBaseAttackClick);
        mainAbility.onClick.AddListener(OnMainAbilityClick);
        secondaryAbility.onClick.AddListener(OnSecondaryAbilityClick);

        wrapper.SetActive(false);
        skillTooltip.gameObject.SetActive(false);
    }

    public void UpdateUnit(PlayerController player)
    {
        if (currentPlayer != null) currentPlayer.ControlModeChanged.RemoveListener(UpdateControlMode);
        currentPlayer = player;
        currentPlayer.ControlModeChanged.AddListener(UpdateControlMode);

        Unit unit = currentPlayer.CurrentUnit;

        ClearSkills();
        if (unit != null)
        {
            UpdateTeamColors();
            UpdatePortrait();
            UpdateHealth();
            UpdateShield();
            UpdateSkills();

            wrapper.SetActive(true);
        } else
        {
            wrapper.SetActive(false);
        }
    }

    public void ShowTooltip(SkillConfig config, UnitAction action)
    {
        skillTooltip.SetValue(config, action);
        skillTooltip.gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        skillTooltip.gameObject.SetActive(false);
        skillTooltip.SetValue(null, null);
    }

    private void UpdateTeamColors()
    {
        foreach(Image sprite in teamColors)
        {
            sprite.color = currentPlayer.PlayerColor;
        }
    }

    private void UpdatePortrait()
    {
        Unit unit = currentPlayer.CurrentUnit;
        Sprite unitPortrait = unit.unitClass.portraits[currentPlayer.faction];
        portrait.sprite = unitPortrait;
    }

    private void UpdateHealth()
    {
        Unit unit = currentPlayer.CurrentUnit;
        healthText.text = UIHelpers.FormatHealth(unit.Health, true);
    }

    private void UpdateShield()
    {
        Unit unit = currentPlayer.CurrentUnit;
        shieldText.text = UIHelpers.FormatShield(unit.Shield);
    }

    private void UpdateSkills()
    {
        Unit unit = currentPlayer.CurrentUnit;
        UnitConfig unitClass = unit.unitClass;

        if (unitClass.baseAttack.isActive)
        {
            baseAttackPanel.SetActions(unitClass.baseAttack);
            baseAttackPanel.gameObject.SetActive(true);
        }

        if (unitClass.mainAbility.isActive)
        {
            mainAbilityPanel.SetActions(unitClass.mainAbility);
            mainAbilityPanel.gameObject.SetActive(true);
        }

        if (unitClass.secondaryAbility.isActive)
        {
            secondaryAbilityPanel.SetActions(unitClass.secondaryAbility);
            secondaryAbilityPanel.gameObject.SetActive(true);
        }
    }

    private void ClearSkills()
    {
        baseAttackPanel.SetActions(null);
        mainAbilityPanel.SetActions(null);
        secondaryAbilityPanel.SetActions(null);

        baseAttackPanel.gameObject.SetActive(false);
        mainAbilityPanel.gameObject.SetActive(false);
        secondaryAbilityPanel.gameObject.SetActive(false);
    }

    private void OnBaseAttackClick()
    {
        if (currentPlayer != null)
            currentPlayer.ChangeAttackMode(AttackModes.Attack);
    }

    private void OnMainAbilityClick()
    {
        if (currentPlayer != null)
            currentPlayer.ChangeAttackMode(AttackModes.MainAbility);
    }

    private void OnSecondaryAbilityClick()
    {
        if (currentPlayer != null)
            currentPlayer.ChangeAttackMode(AttackModes.SecondaryAbility);
    }

    void UpdateControlMode()
    {
        ChangeButtonColor(baseAttack, Color.white);
        ChangeButtonColor(mainAbility, Color.white);
        ChangeButtonColor(secondaryAbility, Color.white);

        if (currentPlayer.AttackMode == AttackModes.Attack) ChangeButtonColor(baseAttack, Color.green);
        if (currentPlayer.AttackMode == AttackModes.MainAbility) ChangeButtonColor(mainAbility, Color.green);
        if (currentPlayer.AttackMode == AttackModes.SecondaryAbility) ChangeButtonColor(secondaryAbility, Color.green);
    }   

    private void ChangeButtonColor(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 0.9f;
        colors.pressedColor = color * 0.6f;
        colors.selectedColor = color * 0.9f;
        colors.disabledColor = color * 0.6f;
        button.colors = colors;
    }
}
