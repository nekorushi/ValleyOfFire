using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnsCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private Image fill;

    [SerializeField] private Color fullColor;
    [SerializeField] private Color emptyColor;

    [SerializeField] private int turnsLimit;

    private int _turnsLeft;
    public int TurnsLeft { get { return _turnsLeft; } }

    private void Awake()
    {
        _turnsLeft = -1;
        fill.color = fullColor;
    }


    public void TriggerNewTurn()
    {
        if (IsFirstTurn())
        {
            _turnsLeft = turnsLimit;
        } else
        {
            _turnsLeft = Mathf.Clamp(_turnsLeft - 1, 0, turnsLimit);
        }

        SetValue(_turnsLeft);
    }

    public bool IsFirstTurn()
    {
        return _turnsLeft == -1;
    }

    public bool IsLastTurn()
    {
        return _turnsLeft == 1;
    }

    public bool HasNoMoreTurns()
    {
        return _turnsLeft == 0;
    }

    private void SetValue(int value)
    {
        counterText.text = value.ToString();
        UpdateFill(value);
    }

    private void UpdateFill(int value)
    {
        fill.color = Color.Lerp(emptyColor, fullColor, EaseIn((float)value / turnsLimit));
    }

    public static float EaseIn(float t)
    {
        return t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
    }
}
