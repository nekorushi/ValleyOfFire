using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private PlayerController[] players;

    [SerializeField]
    private GameplayUI gameplayUI;

    private void Start()
    {
        StartCoroutine(BeginRound());
    }

    private IEnumerator BeginRound()
    {
        int currentPlayerIdx = 0;
        while (CheckWinningConditions())
        {
            PlayerController currentPlayer = players[currentPlayerIdx];
            yield return PerformPlayerTurn(currentPlayer);
            currentPlayerIdx = GetNextPlayer(currentPlayerIdx);
            yield return null;
        }
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
        Debug.Log("Checking winning conditions");
        return true;
    }
}
