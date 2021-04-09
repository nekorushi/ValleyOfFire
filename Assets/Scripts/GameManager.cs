using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }

            return _instance;
        }
    }

    [SerializeField]
    private PlayerController[] players;

    [SerializeField]
    private GameplayUI gameplayUI;

    [SerializeField]
    private RectTransform summaryPanel;

    [SerializeField]
    private Text summaryWinner;

    [SerializeField]
    private List<LevelTile> tickingTiles;

    [SerializeField]
    private TurnsCounter turnsCounter;

    private void Start()
    {
        summaryPanel.gameObject.SetActive(false);

        StartCoroutine(BeginRound());
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private IEnumerator BeginRound()
    {
        yield return StartCoroutine(gameplayUI.PlayIntro());

        int currentPlayerIdx = 0;
        while (!CheckWinningConditions())
        {
            bool isNewTurn = currentPlayerIdx == 0;
            if (isNewTurn)
            {
                foreach(LevelTile tile in tickingTiles) { 
                    tile.OnTick();
                }

                turnsCounter.TriggerNewTurn();
            }

            PlayerController currentPlayer = players[currentPlayerIdx];
            yield return PerformPlayerTurn(currentPlayer);
            currentPlayerIdx = GetNextPlayer(currentPlayerIdx);
            yield return null;
        }

        summaryWinner.text = GetStrongestPlayer().PlayerName;
        summaryPanel.gameObject.SetActive(true);
    }

    private int GetNextPlayer(int currentPlayerIdx)
    {
        int newPlayerIdx = currentPlayerIdx + 1;
        return newPlayerIdx >= players.Length ? 0 : newPlayerIdx;
    }

    private IEnumerator PerformPlayerTurn(PlayerController player)
    {
        gameplayUI.ActivePlayer = player;
        yield return StartCoroutine(player.PerformTurn());
    }

    public bool CheckWinningConditions()
    {
        int alivePlayers = players.Where(player => player.HasAliveFireUnits).ToList().Count;
        if (alivePlayers <= 1) return true;

        bool turnsLimitReached = turnsCounter.TurnsLeft <= 0;
        if (turnsLimitReached)
        {
            return GetStrongestPlayer() != null;
        }

        return false;
    }

    private PlayerController GetStrongestPlayer()
    {
        float strongestPlayerHealth = players.Max(player => player.AllUnitsHealth);
        bool strongestPlayerExists = players.Count(players => players.AllUnitsHealth == strongestPlayerHealth) < 2;

        if (strongestPlayerExists) return players.First(players => players.AllUnitsHealth == strongestPlayerHealth);
        return null;
    }
}
