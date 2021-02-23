using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private PlayerController[] players;

    [SerializeField]
    private GameplayUI gameplayUI;

    [SerializeField]
    private RectTransform summaryPanel;

    [SerializeField]
    private Text summaryWinner;

    private void Start()
    {
        summaryPanel.gameObject.SetActive(false);
        StartCoroutine(BeginRound());
    }

    private IEnumerator BeginRound()
    {
        int currentPlayerIdx = 0;
        while (!CheckWinningConditions())
        {
            PlayerController currentPlayer = players[currentPlayerIdx];
            yield return PerformPlayerTurn(currentPlayer);
            currentPlayerIdx = GetNextPlayer(currentPlayerIdx);
            yield return null;
        }

        summaryWinner.text = players.FirstOrDefault(player => player.HasAliveUnits).name;
        summaryPanel.gameObject.SetActive(true);
    }

    private int GetNextPlayer(int currentPlayerIdx)
    {
        int newPlayerIdx = currentPlayerIdx + 1;
        return newPlayerIdx >= players.Length ? 0 : newPlayerIdx;
    }

    private IEnumerator PerformPlayerTurn(PlayerController player)
    {
        gameplayUI.activePlayer = player;
        yield return StartCoroutine(player.PerformTurn());
    }

    private bool CheckWinningConditions()
    {
        int alivePlayers = players.Where(player => player.HasAliveUnits).ToList().Count;
        return alivePlayers <= 1;
    }
}
