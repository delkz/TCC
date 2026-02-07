using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridMapAsset))]
public class GridMapAssetEditor : Editor
{
    private GridMapAsset map;

    private CellType activePaintType = CellType.Path;
    private bool isPainting;

    private const float CELL_WIDTH = 60f;
    private const float CELL_HEIGHT = 25f;

    private void OnEnable()
    {
        map = (GridMapAsset)target;
    }

    public override void OnInspectorGUI()
    {
        // ===== HEADER =====
        EditorGUILayout.LabelField("Grid Map Asset", EditorStyles.boldLabel);
        GUILayout.Space(5);

        map.width = EditorGUILayout.IntField("Width", map.width);
        map.height = EditorGUILayout.IntField("Height", map.height);
        map.theme = (GridTheme)EditorGUILayout.ObjectField(
            "Theme",
            map.theme,
            typeof(GridTheme),
            false
        );

        GUILayout.Space(10);

        if (GUILayout.Button("Initialize / Resize Grid"))
        {
            Undo.RecordObject(map, "Resize Grid");
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

        GUILayout.Space(15);

        DrawPaintToolbar();

        GUILayout.Space(10);

        DrawGridEditor();

        EditorUtility.SetDirty(map);
    }

    // =========================
    // TOOLBAR DE PINTURA
    // =========================
    private void DrawPaintToolbar()
    {
        EditorGUILayout.LabelField("Paint Mode", EditorStyles.boldLabel);

        activePaintType = (CellType)GUILayout.Toolbar(
            (int)activePaintType,
            System.Enum.GetNames(typeof(CellType))
        );
    }

    // =========================
    // GRID
    // =========================
    private void DrawGridEditor()
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            isPainting = true;
            e.Use();
        }

        if (e.type == EventType.MouseUp)
        {
            isPainting = false;
        }

        for (int y = map.height - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();

            for (int x = 0; x < map.width; x++)
            {
                int index = x + y * map.width;
                CellType current = map.cells[index];

                Rect cellRect = GUILayoutUtility.GetRect(CELL_WIDTH, CELL_HEIGHT);

                GUI.color = GetColor(current);
                GUI.Box(cellRect, current.ToString());
                GUI.color = Color.white;

                if (isPainting && cellRect.Contains(e.mousePosition))
                {
                    if (current != activePaintType)
                    {
                        Undo.RecordObject(map, "Paint Cell");
                        map.SetCell(x, y, activePaintType);
                        EditorUtility.SetDirty(map);
                        Repaint();
                    }
                }
            }

            GUILayout.EndHorizontal();
        }
    }

    // =========================
    // VISUAL
    // =========================
    private Color GetColor(CellType type)
    {
        return type switch
        {
            CellType.Path => new Color(0.9f, 0.8f, 0.3f),
            CellType.Spawn => new Color(0.3f, 0.8f, 0.3f),
            CellType.Goal => new Color(0.9f, 0.3f, 0.3f),
            CellType.Blocked => Color.black,
            _ => Color.gray
        };
    }
}
