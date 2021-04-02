using UnityEngine;

public class GameBackground : MonoBehaviour
{
    private SpriteRenderer backgroundImage;

    private void Awake()
    {
        backgroundImage = GetComponent<SpriteRenderer>();
        GraphicsToggle.Instance.DesignerModeChanged.AddListener(OnDesignerModeChange);
    }

    private void OnDesignerModeChange()
    {
        Color newColor = backgroundImage.color;
        newColor.a = GraphicsToggle.Instance.DesignerMode ? 0 : 1;
        backgroundImage.color = newColor;
    }
}
