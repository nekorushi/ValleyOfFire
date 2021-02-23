using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private GameplayUI gameplayUI;

    [SerializeField]
    private PlayerController player;

    private void Start()
    {
        Debug.Log("UI Started");
        gameplayUI.ActivePlayerChanged.AddListener(OnPlayerChange);
    }

    private void OnPlayerChange()
    {
        Debug.Log("OnPlayerChange");
        GetComponent<RectTransform>().localScale = gameplayUI.activePlayer == player ? Vector3.one: Vector3.zero;
    }
}
