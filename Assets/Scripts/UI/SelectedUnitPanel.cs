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
    private Button primaryAttack;

    [SerializeField]
    private Button secondaryAttack;

    private void Awake()
    {
        primaryAttack.onClick.AddListener(OnPrimaryAttackClick);
        secondaryAttack.onClick.AddListener(OnSecondaryAttackClick);
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
        shieldText.text = unit.Shield.ToString();
    }

    private void OnPrimaryAttackClick()
    {
        if (currentPlayer != null)
            currentPlayer.ChangeAttackMode(AttackModes.Primary);
    }

    private void OnSecondaryAttackClick()
    {
        if (currentPlayer != null)
            currentPlayer.ChangeAttackMode(AttackModes.Secondary);
    }

    void UpdateControlMode()
    {
        ChangeButtonColor(primaryAttack, Color.white);
        ChangeButtonColor(secondaryAttack, Color.white);

        if (currentPlayer.AttackMode == AttackModes.Primary) ChangeButtonColor(primaryAttack, Color.green);
        if (currentPlayer.AttackMode == AttackModes.Secondary) ChangeButtonColor(secondaryAttack, Color.green);
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
