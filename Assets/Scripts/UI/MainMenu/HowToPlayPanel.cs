using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HowToPlayPanel : MonoBehaviour
{
    [SerializeField] private GameObject StepOne;
    [SerializeField] private GameObject StepTwo;
    [SerializeField] private GameObject StepThree;
    [SerializeField] private GameObject StepFour;
    [SerializeField] private GameObject StepFive;

    private void Awake()
    {
        Close();
    }

    public void Open()
    {
        SwitchView(StepOne);
    }

    public void Close()
    {
        SwitchView(null);
    }

    public void SwitchView(GameObject targetStep)
    {
        List<GameObject> steps = new List<GameObject>()
        {
            StepOne,
            StepTwo,
            StepThree,
            StepFour,
            StepFive
        };

        foreach (GameObject step in steps)
        {
            step.SetActive(step == targetStep);
        }
    }
}
