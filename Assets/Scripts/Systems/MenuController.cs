using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void PlayLevel(LevelData level)
    {
        GameSession.Instance.SelectLevel(level);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
