using UnityEngine;
using TMPro;

public class UILog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float lifeTime = 10f;

    private void Awake()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetText(string value)
    {
        text.text = value;
    }
}
