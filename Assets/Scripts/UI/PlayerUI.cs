using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private GameplayUI gameplayUI;

    [SerializeField]
    private PlayerController player;

    [SerializeField]
    private Text playerNameLabel;

    [SerializeField]
    private Button primaryAttack;

    [SerializeField]
    private Text primaryAttackLabel;

    [SerializeField]
    private Button secondaryAttack;

    [SerializeField]
    private Text secondaryAttackLabel;

    private void OnEnable()
    {
        gameplayUI.ActivePlayerChanged.AddListener(OnPlayerChange);
        player.ControlModeChanged.AddListener(UpdateControlMode);

        playerNameLabel.text = player.name;
        playerNameLabel.color = player.PlayerColor;

        primaryAttack.onClick.AddListener(delegate { player.ChangeAttackMode(AttackModes.Primary); });
        secondaryAttack.onClick.AddListener(delegate { player.ChangeAttackMode(AttackModes.Secondary); });
        UpdateControlMode();
    }

    private void OnPlayerChange()
    {
        GetComponent<RectTransform>().localScale = gameplayUI.activePlayer == player ? Vector3.one: Vector3.zero;
    }

    void UpdateControlMode()
    {
        ChangeButtonColor(primaryAttack, Color.white);
        ChangeButtonColor(secondaryAttack, Color.white);

        if (player.AttackMode == AttackModes.Primary) ChangeButtonColor(primaryAttack, Color.green);
        if (player.AttackMode == AttackModes.Secondary) ChangeButtonColor(secondaryAttack, Color.green);
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
