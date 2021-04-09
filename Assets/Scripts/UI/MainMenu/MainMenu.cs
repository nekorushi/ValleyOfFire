using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private HowToPlayPanel howToPlayPanel;

    [SerializeField] private CanvasGroup uiCanvas;
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private Material logoFireMaterial;

    void Start()
    {
        startButton.onClick.AddListener(() => { StartCoroutine(StartGame()); });
        howToPlayButton.onClick.AddListener(HowToPlay);
        exitButton.onClick.AddListener(ExitGame);
    }


    private IEnumerator StartGame()
    {
        float elapsedTime = 0f;
        float transitionDuration = 1f;

        while (elapsedTime <= transitionDuration)
        {
            float progress = elapsedTime / transitionDuration;

            uiCanvas.alpha = 1 - progress;
            musicPlayer.volume = 1 - progress;
            logoFireMaterial.SetFloat("_Alpha", 1 - progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(.5f);

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
