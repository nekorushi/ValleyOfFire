using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private HowToPlayPanel howToPlayPanel;


    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        howToPlayButton.onClick.AddListener(HowToPlay);
        exitButton.onClick.AddListener(ExitGame);
    }


    private void StartGame()
    {
        SceneManager.LoadScene("Combat");
    }


    private void HowToPlay()
    {
        howToPlayPanel.Open();
    }


    private void ExitGame()
    {
        Application.Quit();
    }
}
