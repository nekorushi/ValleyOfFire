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
    private Button controlMode;

    [SerializeField]
    private Text controlModeLabel;

    [SerializeField]
    private Button attackMode;

    [SerializeField]
    private Text attackModeLabel;

    private void OnEnable()
    {
        gameplayUI.ActivePlayerChanged.AddListener(OnPlayerChange);
        player.ControlModeChanged.AddListener(UpdateControlMode);

        playerNameLabel.text = player.name;
        playerNameLabel.color = player.PlayerColor;

        controlMode.onClick.AddListener(player.ChangeControlMode);
        attackMode.onClick.AddListener(player.ChangeAttackMode);
        UpdateControlMode();
    }

    private void OnPlayerChange()
    {
        GetComponent<RectTransform>().localScale = gameplayUI.activePlayer == player ? Vector3.one: Vector3.zero;
    }

    void UpdateControlMode()
    {
        Dictionary<ControlModes, string> controlLabels = new Dictionary<ControlModes, string>()
        {
            {ControlModes.Movement, "Movement"},
            {ControlModes.Attack, "Attack"}
        };

        Dictionary<AttackModes, string> attackLabels = new Dictionary<AttackModes, string>()
        {
            {AttackModes.Primary, "Primary Attack"},
            {AttackModes.Secondary, "Secondary Attack"}
        };

        controlModeLabel.text = controlLabels[player.ControlMode];
        attackModeLabel.text = attackLabels[player.AttackMode];
    }
}
