using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedUnitPanel : MonoBehaviour
{
    private PlayerController currentPlayer;

    [SerializeField]
    private GameObject wrapper;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Image portrait;

    [SerializeField]
    private TMP_Text healthText;

    [SerializeField]
    private TMP_Text shieldText;

    [SerializeField]
    private Button baseAttack;

    [SerializeField]
    private Button mainAbility;

    [SerializeField]
    private Button secondaryAbility;

    private void Awake()
    {
        baseAttack.onClick.AddListener(OnBaseAttackClick);
        mainAbility.onClick.AddListener(OnMainAbilityClick);
        mainAbility.onClick.AddListener(OnSecondaryAbilityClick);
        wrapper.SetActive(false);
    }

    public void UpdateUnit(PlayerController player)
    {
        if (currentPlayer != null) currentPlayer.ControlModeChanged.RemoveListener(UpdateControlMode);
        currentPlayer = player;
        currentPlayer.ControlModeChanged.AddListener(UpdateControlMode);

        Unit unit = currentPlayer.CurrentUnit;

        if (unit != null)
        {
            UpdateBackground();
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

    private void UpdateBackground()
    {
        background.color = currentPlayer.PlayerColor;
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
        healthText.text = UIHelpers.FormatHealth(unit.Health);
    }

    private void UpdateShield()
    {
        Unit unit = currentPlayer.CurrentUnit;
        shieldText.text = UIHelpers.FormatShield(unit.Shield);
    }

    private void UpdateSkills()
    {
        Unit unit = currentPlayer.CurrentUnit;
        baseAttack.gameObject.SetActive(unit.unitClass.baseAttack.isActive);
        mainAbility.gameObject.SetActive(unit.unitClass.mainAbility.isActive);
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
