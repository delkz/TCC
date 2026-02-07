using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.pKey.wasPressedThisFrame)
        {
            GameManager.Instance.TogglePause();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GameManager.Instance.ToggleSpeed();
        }
    }

    
}
