using UnityEngine;
using UnityEngine.UI;

public class AdvantageBar : MonoBehaviour
{
    [SerializeField] private PlayerController firstPlayer;
    [SerializeField] private PlayerController secondPlayer;

    [SerializeField] private RectTransform firstBar;
    [SerializeField] private RectTransform secondBar;

    private void Awake()
    {
        firstBar.GetComponent<Image>().color = firstPlayer.PlayerColor;
        secondBar.GetComponent<Image>().color = secondPlayer.PlayerColor;

        firstPlayer.UnitsHealthChanged.AddListener(UpdateBars);
        secondPlayer.UnitsHealthChanged.AddListener(UpdateBars);
    }

    private void UpdateBars()
    {
        float firstPlayerHealth = firstPlayer.AllUnitsHealth;
        float secondPlayerHealth = secondPlayer.AllUnitsHealth;

        float barPosition = firstPlayerHealth / (firstPlayerHealth + secondPlayerHealth);

        firstBar.anchorMax = new Vector2(barPosition, firstBar.anchorMax.y);
        secondBar.anchorMin = new Vector2(barPosition, secondBar.anchorMin.y);
    }
}
