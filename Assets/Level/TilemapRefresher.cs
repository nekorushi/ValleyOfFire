using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapRefresher : MonoBehaviour
{
    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        GraphicsToggle.Instance.DesignerModeChanged.AddListener(OnDesignerModeChange);
    }

    private void OnDesignerModeChange()
    {
        tilemap.RefreshAllTiles();
    }
}
