using UnityEngine;
using UnityEngine.InputSystem;

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

    private static readonly Color VALID_COLOR = new(0f, 1f, 0f, 0.5f);
    private static readonly Color INVALID_COLOR = new(1f, 0f, 0f, 0.5f);

    private BuildingData CurrentBuilding => buildings[selectedIndex];

    private void Start()
    {
        CreatePreview();

        hudController.UpdateBuilding(
            CurrentBuilding.buildingName,
            CurrentBuilding.buildCost
        );
    }

    private void Update()
    {
        HandleModeSwitch();
        UpdatePreview();
        HandleInput();
    }

    private void HandleInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentMode == BuildMode.Build)
                TryBuild();
            else
                TryDestroy();
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
            currentMode = currentMode == BuildMode.Build
                ? BuildMode.Destroy
                : BuildMode.Build;

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

        gridManager.SetCellOccupant(currentGridPos, building);
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
        Destroy(occupant);
        gridManager.SetCellOccupant(currentGridPos, null);
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
