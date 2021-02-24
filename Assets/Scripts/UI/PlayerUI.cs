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

    private void OnEnable()
    {
        gameplayUI.ActivePlayerChanged.AddListener(OnPlayerChange);
        player.ControlModeChanged.AddListener(UpdateControlMode);

        playerNameLabel.text = player.name;
        playerNameLabel.color = player.PlayerColor;
        controlMode.onClick.AddListener(player.ChangeControlMode);
        UpdateControlMode();
    }

    private void OnPlayerChange()
    {
        GetComponent<RectTransform>().localScale = gameplayUI.activePlayer == player ? Vector3.one: Vector3.zero;
    }

    void UpdateControlMode()
    {
        Dictionary<ControlModes, string> labels = new Dictionary<ControlModes, string>()
        {
            {ControlModes.Movement, "Movement"},
            {ControlModes.Attack, "Attack"}
        };

        controlModeLabel.text = labels[player.ControlMode];
    }
}
