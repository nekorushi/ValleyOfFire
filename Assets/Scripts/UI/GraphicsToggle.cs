using UnityEngine;
using UnityEngine.Events;

public class GraphicsToggle : MonoBehaviour
{

    public UnityEvent DesignerModeChanged;

    private static GraphicsToggle _instance;
    public static GraphicsToggle Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<GraphicsToggle>();
            return _instance;
        }
    }

    private bool _designerMode;
    public bool DesignerMode { get { return _designerMode; } }

    public void ChangeMode()
    {
        _designerMode = !_designerMode;
        DesignerModeChanged.Invoke();
    }
}
