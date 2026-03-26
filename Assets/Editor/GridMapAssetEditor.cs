using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(GridMapAsset))]
public class GridMapAssetEditor : Editor
{
    private GridMapAsset map;

    private CellType activePaintType = CellType.Path;
    private bool isPainting;
    private CellType[] paintTypes;
    private GUIContent[] paintTypeLabels;

    private const float CELL_WIDTH = 60f;
    private const float CELL_HEIGHT = 25f;

    private void OnEnable()
    {
        map = (GridMapAsset)target;
        RefreshPaintTypes();
    }

    private void RefreshPaintTypes()
    {
        Array rawValues = Enum.GetValues(typeof(CellType));
        paintTypes = new CellType[rawValues.Length];
        paintTypeLabels = new GUIContent[rawValues.Length];

        for (int i = 0; i < rawValues.Length; i++)
        {
            CellType type = (CellType)rawValues.GetValue(i);
            paintTypes[i] = type;
            paintTypeLabels[i] = new GUIContent(type.ToString());
        }

        if (Array.IndexOf(paintTypes, activePaintType) < 0 && paintTypes.Length > 0)
        {
            activePaintType = paintTypes[0];
        }
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

        if (paintTypes == null || paintTypes.Length == 0)
        {
            EditorGUILayout.HelpBox("Nenhum CellType disponivel para pintura.", MessageType.Warning);
            return;
        }

        int selectedIndex = Array.IndexOf(paintTypes, activePaintType);
        if (selectedIndex < 0)
        {
            selectedIndex = 0;
        }

        selectedIndex = GUILayout.Toolbar(selectedIndex, paintTypeLabels);
        activePaintType = paintTypes[selectedIndex];
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
        if (map != null && map.theme != null)
        {
            if (TryGetThemeColor(type, out Color themedColor))
            {
                return themedColor;
            }
        }

        return GetGeneratedFallbackColor(type);
    }

    private bool TryGetThemeColor(CellType type, out Color color)
    {
        GridTheme theme = map.theme;

        if (theme == null)
        {
            color = default;
            return false;
        }

        return theme.TryGetColor(type, out color);
    }

    private Color GetGeneratedFallbackColor(CellType type)
    {
        int seed = Mathf.Abs((int)type) + 1;
        float hue = (seed * 0.173f) % 1f;
        return Color.HSVToRGB(hue, 0.45f, 0.85f);
    }
}
