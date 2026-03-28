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
    private const float BORDER_WIDTH = 1f;
    private const int ASPECT_RATIO_WIDTH = 16;
    private const int ASPECT_RATIO_HEIGHT = 9;
    private const float MARGIN = 1f; // 1 célula de margem

    private Vector2 gridScrollPosition = Vector2.zero;

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
        // Calcular tamanho necessário para o grid
        float gridWidth = map.width * CELL_WIDTH;
        float gridHeight = map.height * CELL_HEIGHT;

        // Obter rect para o scroll view
        Rect scrollViewRect = EditorGUILayout.GetControlRect(GUILayout.Height(400));

        // Usar GUI.BeginScrollView com tamanho fixo
        gridScrollPosition = GUI.BeginScrollView(
            scrollViewRect,
            gridScrollPosition,
            new Rect(0, 0, gridWidth, gridHeight),
            true,
            true
        );

        for (int y = 0; y < map.height; y++)
        {
            for (int x = 0; x < map.width; x++)
            {
                int index = x + y * map.width;
                CellType current = map.cells[index];

                Rect cellRect = new Rect(x * CELL_WIDTH, y * CELL_HEIGHT, CELL_WIDTH, CELL_HEIGHT);

                GUI.color = GetColor(current);
                
                // GUI.RepeatButton retorna true enquanto o mouse está pressionado sobre o botão
                if (GUI.RepeatButton(cellRect, current.ToString()))
                {
                    if (current != activePaintType)
                    {
                        Undo.RecordObject(map, "Paint Cell");
                        map.SetCell(x, y, activePaintType);
                        EditorUtility.SetDirty(map);
                    }
                }

                // Desenhar borda para célula central
                if (IsCenterCell(x, y))
                {
                    DrawCenterCellBorder(cellRect);
                }

                GUI.color = Color.white;
            }
        }

        // Desenhar borda da área 16/9
        Draw169AspectRatioBorder();

        GUI.EndScrollView();
    }

    private bool IsCenterCell(int x, int y)
    {
        int centerX = map.width / 2;
        int centerY = map.height / 2;
        return x == centerX && y == centerY;
    }

    private void DrawCenterCellBorder(Rect cellRect)
    {
        Color borderColor = Color.yellow;
        Handles.color = borderColor;
        Handles.DrawSolidRectangleWithOutline(cellRect, Color.clear, borderColor);
    }

    private void Draw169AspectRatioBorder()
    {
        int centerX = map.width / 2;
        int centerY = map.height / 2;

        // Calcular posição inicial (canto superior esquerdo da área 16/9)
        float areaWidth = ASPECT_RATIO_WIDTH * CELL_WIDTH + MARGIN * 2;
        float areaHeight = ASPECT_RATIO_HEIGHT * CELL_HEIGHT + MARGIN * 2;

        float startX = centerX * CELL_WIDTH - areaWidth / 2;
        float startY = centerY * CELL_HEIGHT - areaHeight / 2;

        Rect areaBorder = new Rect(startX, startY, areaWidth, areaHeight);

        // Desenhar borda da área 16/9
        Color aspectRatioColor = new Color(0.2f, 0.8f, 1f, 1f); // Azul claro
        Handles.color = aspectRatioColor;
        Handles.DrawSolidRectangleWithOutline(areaBorder, Color.clear, aspectRatioColor);
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
