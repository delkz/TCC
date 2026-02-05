using UnityEngine;

public class UILogger : MonoBehaviour
{
    public static UILogger Instance { get; private set; }

    [SerializeField] private GameObject consoleLogPrefab;
    [SerializeField] private Transform consoleContainer;

    [SerializeField] private int maxLogs = 20;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void Log(string message)
    {
        if (Instance == null)
            return;

        Instance.LogInternal(message);
    }

    private void LogInternal(string message)
    {
        GameObject logInstance = Instantiate(
            consoleLogPrefab,
            consoleContainer
        );

        UILog uiLog = logInstance.GetComponent<UILog>();
        if (uiLog != null)
        {
            uiLog.SetText(message);
        }

        // limita quantidade de logs
        if (consoleContainer.childCount > maxLogs)
        {
            Destroy(consoleContainer.GetChild(0).gameObject);
        }
    }
}
