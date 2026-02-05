using UnityEngine;
using UnityEngine.InputSystem;

public class GridClickerTester : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            worldPosition.z = 0f;

            Vector2Int gridPosition = gridManager.WorldToGridPosition(worldPosition);

            if (gridManager.IsValidCell(gridPosition.x, gridPosition.y))
            {
                Debug.Log($"Célula clicada: X={gridPosition.x}, Y={gridPosition.y}");
            }
            else
            {
                Debug.Log("Clique fora do grid");
            }
        }
    }

}
