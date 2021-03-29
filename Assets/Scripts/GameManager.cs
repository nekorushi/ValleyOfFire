using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

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
    private TMP_Text screenResolutionText;

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

    private void Start()
    {
        summaryPanel.gameObject.SetActive(false);
        StartCoroutine(BeginRound());

        Camera mainCamera = Camera.main;
        screenResolutionText.text = string.Format("{0}x{1}", mainCamera.pixelWidth, mainCamera.pixelHeight);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private IEnumerator BeginRound()
    {
        int currentPlayerIdx = 0;
        while (!CheckWinningConditions())
        {
            if (currentPlayerIdx == 0)
            {
                foreach(LevelTile tile in tickingTiles)
                {
                    tile.OnTick();
                }
            }

            PlayerController currentPlayer = players[currentPlayerIdx];
            yield return PerformPlayerTurn(currentPlayer);
            currentPlayerIdx = GetNextPlayer(currentPlayerIdx);
            yield return null;
        }

        summaryWinner.text = players.FirstOrDefault(player => player.HasAliveFireUnits).name;
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

    public bool CheckWinningConditions()
    {
        int alivePlayers = players.Where(player => player.HasAliveFireUnits).ToList().Count;
        return alivePlayers <= 1;
    }
}
