using UnityEngine;

public class GridObject : MonoBehaviour
{
    [SerializeField] private Vector2Int size = Vector2Int.one;

    public void AlignToGrid(float cellSize)
    {
        // transform.localScale = new Vector3(
        //     size.x * cellSize,
        //     size.y * cellSize,
        //     1f
        // );
    }
}