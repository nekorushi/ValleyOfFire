using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedUnitPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject wrapper;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Image portrait;

    [SerializeField]
    private TMP_Text healthText;

    [SerializeField]
    private TMP_Text shieldText;

    private void Awake()
    {
        wrapper.SetActive(false);
    }

    public void UpdateUnit(PlayerController player)
    {
        Unit unit = player.CurrentUnit;

        if (unit != null)
        {
            background.color = player.PlayerColor;

            Sprite unitPortrait = unit.portrait[player.faction];
            portrait.sprite = unitPortrait;

            healthText.text = UIHelpers.FormatHealth(unit.Health);
            shieldText.text = unit.Shield.ToString();

            wrapper.SetActive(true);
        } else
        {
            wrapper.SetActive(false);
        }
    }
}
