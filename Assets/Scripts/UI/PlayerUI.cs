using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private GameplayUI gameplayUI;

    [SerializeField]
    private Button nextTurnButton;

    [SerializeField]
    private List<ActiveUnitUI> unitUIs;

    private PlayerController currentPlayer;


    private void OnEnable()
    {
        gameplayUI.ActivePlayerChanged.AddListener(OnPlayerChange);
        nextTurnButton.onClick.AddListener(OnEndTurnButtonClick);
    }

    private void OnPlayerChange()
    {
        if (currentPlayer)
        {
            currentPlayer.TurnStarted.RemoveListener(DisplayActiveUnits);
            currentPlayer.UnitSelectionChanged.RemoveListener(DisplayActiveUnits);
        }

        currentPlayer = gameplayUI.ActivePlayer;
        currentPlayer.TurnStarted.AddListener(DisplayActiveUnits);
        currentPlayer.UnitSelectionChanged.AddListener(DisplayActiveUnits);

        DisplayActiveUnits();
    }

    private void DisplayActiveUnits()
    {
        PlayerTurnManager turnManager = currentPlayer.turnManager;

        foreach (ActiveUnitUI unitUI in unitUIs)
        {
            unitUI.ClearUnit();
        }

        int idx = 0;
        foreach(KeyValuePair<Unit, ActionPoints> activeUnit in turnManager.ActiveUnits)
        {
            ActiveUnitUI unitUI = unitUIs[idx];
            unitUI.SetUnit(currentPlayer, activeUnit.Key, activeUnit.Value, true);
            idx++;
        }

        bool hasSelectedNonActiveUnit = currentPlayer.CurrentUnit
            && !turnManager.ActiveUnits.ContainsKey(currentPlayer.CurrentUnit);
        bool hasFreeSlots = turnManager.ActiveUnits.Count < turnManager.MaxUnitsPerTurn;
        if (hasSelectedNonActiveUnit && hasFreeSlots)
        {
            ActiveUnitUI unitUI = unitUIs[turnManager.ActiveUnits.Count];
            unitUI.SetUnit(currentPlayer, currentPlayer.CurrentUnit, new ActionPoints(true, true), false);
        }
    }

    public void OnEndTurnButtonClick()
    {
        currentPlayer.turnManager.FinishTurn();
    }
}
