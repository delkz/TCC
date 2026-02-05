using UnityEngine;
using UnityEngine.InputSystem;

public class BuildPreviewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Preview")]
    [SerializeField] private GameObject previewPrefab;

    [Header("Buildings")]
    [SerializeField] private GameObject[] buildingPrefabs;
    [SerializeField] private int selectedIndex = 0;

    [Header("Mode")]
    [SerializeField] private BuildMode currentMode = BuildMode.Build;

    [Header("UI")]
    [SerializeField] private BuildHUDController hudController;

    private GameObject previewInstance;
    private SpriteRenderer previewRenderer;

    private Vector2Int currentGridPos;
    private bool canBuild;

    private static readonly Color BUILD_VALID_COLOR = new(0f, 1f, 0f, 0.5f);
    private static readonly Color BUILD_INVALID_COLOR = new(1f, 0f, 0f, 0.5f);
    private static readonly Color DESTROY_COLOR = new(1f, 0f, 0f, 0.4f);

    private void Start()
    {
        CreatePreview();
        hudController.UpdateMode(currentMode);
        hudController.UpdateBuilding(buildingPrefabs[selectedIndex].name);

    }

    private void Update()
    {
        HandleModeSwitch();
        HandleBuildingSelection();
        UpdatePreview();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleActionClick();
        }
    }

    // ================= INPUT =================

    private void HandleModeSwitch()
    {
        if (!Keyboard.current.tabKey.wasPressedThisFrame)
            return;

        currentMode = currentMode == BuildMode.Build
            ? BuildMode.Destroy
            : BuildMode.Build;

        hudController.UpdateMode(currentMode);
        Debug.Log($"Modo atual: {currentMode}");
    }

    private void HandleBuildingSelection()
    {
        if (currentMode == BuildMode.Destroy)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
            ChangeBuilding(-1);

        if (Keyboard.current.eKey.wasPressedThisFrame)
            ChangeBuilding(1);
    }

    private void HandleActionClick()
    {
        if (currentMode == BuildMode.Build)
            TryBuild();
        else
            TryDestroy();
    }

    // ================= PREVIEW =================

    private void UpdatePreview()
    {
        if (previewRenderer == null)
            return;

        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0f;

        currentGridPos = gridManager.WorldToGridPosition(worldPosition);

        if (!gridManager.IsValidCell(currentGridPos.x, currentGridPos.y))
        {
            previewRenderer.color = DESTROY_COLOR;
            canBuild = false;
            return;
        }

        previewInstance.transform.position = GetCellCenter(currentGridPos);

        if (currentMode == BuildMode.Destroy)
        {
            previewRenderer.enabled = false;
            canBuild = false;
            return;
        }

        previewRenderer.enabled = true;

        canBuild =
            gridManager.IsCellBuildable(currentGridPos.x, currentGridPos.y) &&
            gridManager.CanBlockCell(currentGridPos);

        previewRenderer.color = canBuild ? BUILD_VALID_COLOR : BUILD_INVALID_COLOR;
    }

    private Vector3 GetCellCenter(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0f);
    }

    private void CreatePreview()
    {
        previewInstance = Instantiate(previewPrefab);
        previewRenderer = previewInstance.GetComponent<SpriteRenderer>();
    }

    private void ChangeBuilding(int direction)
    {
        selectedIndex += direction;

        if (selectedIndex < 0)
            selectedIndex = buildingPrefabs.Length - 1;
        else if (selectedIndex >= buildingPrefabs.Length)
            selectedIndex = 0;

        hudController.UpdateBuilding(buildingPrefabs[selectedIndex].name);
    }

    // ================= ACTIONS =================

    private void TryBuild()
    {
        if (!canBuild)
            return;

        if (!gridManager.CanBlockCell(currentGridPos))
            return;


        Instantiate(
            buildingPrefabs[selectedIndex],
            GetCellCenter(currentGridPos),
            Quaternion.identity
        );

        gridManager.SetCellOccupied(currentGridPos.x, currentGridPos.y, true);
    }

    private void TryDestroy()
    {
        Vector3 position = GetCellCenter(currentGridPos);

        Collider2D hit = Physics2D.OverlapPoint(
            position,
            LayerMask.GetMask("Default")
        );

        if (hit == null)
            return;

        Destroy(hit.gameObject);
        gridManager.SetCellOccupied(currentGridPos.x, currentGridPos.y, false);
    }

}
