using UnityEngine;
using UnityEngine.UI;

public class NextStepButton : MonoBehaviour
{
    [SerializeField]
    private HowToPlayPanel panel;

    [SerializeField]
    private GameObject nextStep;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);

    }

    private void OnButtonClick()
    {
        panel.SwitchView(nextStep);
    }
}
