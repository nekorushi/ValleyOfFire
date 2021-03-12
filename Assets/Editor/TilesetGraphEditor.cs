using UnityEditor;
using Pathfinding;

[CustomGraphEditor(typeof(TilesetGraph), "Tileset Graph")]
public class TilesetGraphEditor : GraphEditor
{
    // Here goes the GUI
    public override void OnInspectorGUI(NavGraph target)
    {
    }
}