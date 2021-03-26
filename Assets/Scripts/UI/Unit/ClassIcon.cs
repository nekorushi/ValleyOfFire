using UnityEngine;
using UnityEngine.UI;

public class ClassIcon : MonoBehaviour
{
    [SerializeField]
    private Image icon;

    public ClassSpriteDict inGameSprites = new ClassSpriteDict();

    public void SetValue(UnitTypes unitClass)
    {
        if (inGameSprites.ContainsKey(unitClass))
        {
            Sprite sprite = inGameSprites[unitClass];
            icon.sprite = sprite;
        }
    }
}
