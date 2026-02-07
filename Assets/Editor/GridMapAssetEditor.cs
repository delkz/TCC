using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridMapAsset))]
public class GridMapAssetEditor : Editor
{
    private GridMapAsset map;

    private void OnEnable()
    {
        map = (GridMapAsset)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Initialize / Resize Grid"))
        {
            map.Resize();
            EditorUtility.SetDirty(map);
        }

        if (map.cells == null || map.cells.Length != map.width * map.height)
        {
            EditorGUILayout.HelpBox(
                "Grid não inicializada ou com tamanho inválido.",
                MessageType.Warning
            );
            return;
        }

        GUILayout.Space(10);
        DrawGridEditor();
    }

    private void DrawGridEditor()
    {
        for (int y = map.height - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();

            for (int x = 0; x < map.width; x++)
            {
                int index = x + y * map.width;
                CellType current = map.cells[index];

                GUI.backgroundColor = GetColor(current);

                if (GUILayout.Button(current.ToString(), GUILayout.Width(60)))
                {
                    map.cells[index] = GetNextCellType(current);
                    EditorUtility.SetDirty(map);
                }
            }

            GUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;
    }

    private CellType GetNextCellType(CellType current)
    {
        int next = ((int)current + 1) % System.Enum.GetValues(typeof(CellType)).Length;
        return (CellType)next;
    }

    private Color GetColor(CellType type)
    {
        return type switch
        {
            CellType.Path => Color.yellow,
            CellType.Spawn => Color.green,
            CellType.Goal => Color.red,
            CellType.Blocked => Color.black,
            _ => Color.gray
        };
    }
}
