using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private GameplayUI gameplayUI;

    [SerializeField]
    private Image background;

    [SerializeField]
    private TMP_Text playerNameLabel;

    private void OnEnable()
    {
        gameplayUI.ActivePlayerChanged.AddListener(OnPlayerChange);
    }

    private void OnPlayerChange()
    {
        playerNameLabel.text = gameplayUI.activePlayer.name;
        background.color = gameplayUI.activePlayer.PlayerColor;
    }
}
