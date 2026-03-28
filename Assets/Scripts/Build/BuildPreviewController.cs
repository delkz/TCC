using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuildPreviewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GoldManager goldManager;
    [SerializeField] private GameHUDController hudController;

    [Header("Preview")]
    [SerializeField] private GameObject previewPrefab;

    [Header("Buildings")]
    [SerializeField] private BuildingData[] buildings;
    [SerializeField] private int selectedIndex = 0;

    private GameObject previewInstance;
    private SpriteRenderer previewRenderer;
    private BuildMode currentMode = BuildMode.Build;
    private Vector2Int currentGridPos;
    private bool canBuild;

    private IRangeDisplayable currentRangeDisplayable;
    private readonly List<GameObject> rangeCellPreviews = new();

    [Header("Range Preview")]
    [SerializeField, Range(0f, 1.5f)] private float rangeVisualShrinkCells = 0.75f;

    private static readonly Color VALID_COLOR = new(0f, 1f, 0f, 0.5f);
    private static readonly Color INVALID_COLOR = new(1f, 0f, 0f, 0.5f);
    private static readonly Color RANGE_CELL_COLOR = new(0f, 0.8f, 1f, 0.28f);

    private BuildingData CurrentBuilding => buildings[selectedIndex];

    private void Start()
    {
        CreatePreview();
        hudController.UpdateMode(currentMode);

        hudController.UpdateBuilding(
            CurrentBuilding.buildingName,
            CurrentBuilding.buildCost
        );
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Paused)
        {
            if (previewInstance != null && previewInstance.activeSelf)
            {
                previewInstance.SetActive(false);
            }

            hudController.ClearUpgradeFeedback();
            HideAllRangeCellPreviews();
            return;
        }

        if (previewInstance != null && !previewInstance.activeSelf)
        {
            previewInstance.SetActive(true);
        }

        HandleModeSwitch();
        UpdatePreview();
        HandleInput();
        UpdateUpgradeFeedback();
        UpdateRangeDisplay();
        UpdateRangeCellPreviewVisual();
    }

    private void HandleInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentMode == BuildMode.Build)
            {
                TryBuild();
            }
            else if (currentMode == BuildMode.Destroy)
            {
                TryDestroy();
            }
            else
            {
                TryUpgrade();
            }
        }

        if (currentMode == BuildMode.Build)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
                ChangeBuilding(-1);

            if (Keyboard.current.eKey.wasPressedThisFrame)
                ChangeBuilding(1);
        }
    }


    private void HandleModeSwitch()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            currentMode = currentMode switch
            {
                BuildMode.Build => BuildMode.Destroy,
                BuildMode.Destroy => BuildMode.Upgrade,
                _ => BuildMode.Build
            };

            hudController.UpdateMode(currentMode);
        }
    }


    // ================= PREVIEW =================

    private void UpdatePreview()
    {
        if (previewRenderer == null)
            return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        currentGridPos = gridManager.WorldToGridPosition(mouseWorld);

        bool isBuildMode = currentMode == BuildMode.Build;
        if (previewInstance != null)
        {
            previewInstance.SetActive(isBuildMode);
        }

        if (!isBuildMode)
        {
            return;
        }

        if (!gridManager.IsValidCell(currentGridPos.x, currentGridPos.y))
        {
            canBuild = false;
            previewRenderer.color = INVALID_COLOR;
            return;
        }

        previewInstance.transform.position =
            gridManager.GridToWorldCenter(currentGridPos);

        canBuild =
            gridManager.IsCellBuildable(currentGridPos.x, currentGridPos.y) &&
            goldManager.CanAfford(CurrentBuilding.buildCost);

        previewRenderer.color = canBuild ? VALID_COLOR : INVALID_COLOR;
    }

    private void CreatePreview()
    {
        previewInstance = Instantiate(previewPrefab);
        previewRenderer = previewInstance.GetComponent<SpriteRenderer>();
    }

    // ================= BUILD =================

    private void TryBuild()
    {
        if (!canBuild)
            return;

        if (!goldManager.Spend(CurrentBuilding.buildCost))
            return;

        GameObject building = Instantiate(
            CurrentBuilding.prefab,
            gridManager.GridToWorldCenter(currentGridPos),
            Quaternion.identity
        );

        if(CurrentBuilding.icon != null)
        {
            SpriteRenderer sr = building.GetComponent<SpriteRenderer>();
            if(sr != null)
                sr.sprite = CurrentBuilding.icon;
        }

        gridManager.SetCellOccupant(currentGridPos, building);
        GameAudioEvents.RaiseBuildPlaced(building.transform.position);
    }

    private void TryDestroy()
    {
        GameObject occupant = gridManager.GetCellOccupant(currentGridPos);
        if (occupant == null)
            return;

        Buildable buildable = occupant.GetComponent<Buildable>();
        if (buildable == null || !buildable.CanBeDestroyed)
            return;

        goldManager.Add(buildable.GetRefundValue());
        Vector3 removedPosition = occupant.transform.position;
        Destroy(occupant);
        gridManager.SetCellOccupant(currentGridPos, null);
        GameAudioEvents.RaiseBuildRemoved(removedPosition);
    }

    private void TryUpgrade()
    {
        GameObject occupant = gridManager.GetCellOccupant(currentGridPos);
        if (occupant == null)
            return;

        Buildable buildable = occupant.GetComponent<Buildable>();
        if (buildable == null || !buildable.CanUpgrade)
            return;

        buildable.TryUpgrade(goldManager);
    }

    private void UpdateUpgradeFeedback()
    {
        if (currentMode != BuildMode.Upgrade)
        {
            hudController.ClearUpgradeFeedback();
            return;
        }

        GameObject occupant = gridManager.GetCellOccupant(currentGridPos);
        if (occupant == null)
        {
            hudController.ClearUpgradeFeedback();
            return;
        }

        Buildable buildable = occupant.GetComponent<Buildable>();
        hudController.UpdateUpgradeFeedback(buildable, goldManager.CurrentGold);
    }

    private void UpdateRangeDisplay()
    {
        GameObject occupant = gridManager.GetCellOccupant(currentGridPos);
        if (occupant == null)
        {
            currentRangeDisplayable = null;
            return;
        }

        currentRangeDisplayable = occupant.GetComponent<IRangeDisplayable>();
    }

    private void UpdateRangeCellPreviewVisual()
    {
        if (currentRangeDisplayable == null)
        {
            HideAllRangeCellPreviews();
            return;
        }

        float range = currentRangeDisplayable.GetDisplayRange();
        if (range <= 0f)
        {
            HideAllRangeCellPreviews();
            return;
        }

        Vector3 centerWorld = currentRangeDisplayable.GetDisplayPosition();
        Vector2Int centerGrid = gridManager.WorldToGridPosition(centerWorld);
        float cellSize = Mathf.Max(0.0001f, gridManager.CellSize);
        int radiusCells = Mathf.CeilToInt(range / cellSize);

        // Compensa o preenchimento da célula inteira para aproximar da área real de alcance.
        float effectiveRange = Mathf.Max(0f, range - cellSize * rangeVisualShrinkCells);

        int previewIndex = 0;
        for (int y = centerGrid.y - radiusCells; y <= centerGrid.y + radiusCells; y++)
        {
            for (int x = centerGrid.x - radiusCells; x <= centerGrid.x + radiusCells; x++)
            {
                if (!gridManager.IsValidCell(x, y))
                {
                    continue;
                }

                Vector2Int cellPos = new Vector2Int(x, y);
                Vector3 cellCenter = gridManager.GridToWorldCenter(cellPos);

                if (Vector2.Distance(cellCenter, centerWorld) > effectiveRange)
                {
                    continue;
                }

                GameObject rangePreview = GetOrCreateRangeCellPreview(previewIndex);
                rangePreview.transform.position = cellCenter;
                rangePreview.SetActive(true);
                previewIndex++;
            }
        }

        for (int i = previewIndex; i < rangeCellPreviews.Count; i++)
        {
            rangeCellPreviews[i].SetActive(false);
        }
    }

    private GameObject GetOrCreateRangeCellPreview(int index)
    {
        while (rangeCellPreviews.Count <= index)
        {
            GameObject previewCell = Instantiate(previewPrefab, transform);
            previewCell.name = $"RangePreview_{rangeCellPreviews.Count}";

            SpriteRenderer sprite = previewCell.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.color = RANGE_CELL_COLOR;
                sprite.sortingOrder = 450;
            }

            Collider2D col = previewCell.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }

            previewCell.SetActive(false);
            rangeCellPreviews.Add(previewCell);
        }

        return rangeCellPreviews[index];
    }

    private void HideAllRangeCellPreviews()
    {
        for (int i = 0; i < rangeCellPreviews.Count; i++)
        {
            if (rangeCellPreviews[i] != null)
            {
                rangeCellPreviews[i].SetActive(false);
            }
        }
    }

    private void ChangeBuilding(int direction)
    {
        selectedIndex += direction;

        if (selectedIndex < 0)
            selectedIndex = buildings.Length - 1;
        else if (selectedIndex >= buildings.Length)
            selectedIndex = 0;

        hudController.UpdateBuilding(
            CurrentBuilding.buildingName,
            CurrentBuilding.buildCost
        );
    }
}
