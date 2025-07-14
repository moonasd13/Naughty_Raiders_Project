using UnityEngine;
using TMPro;

public class DebugUIManager : MonoBehaviour
{
    public static DebugUIManager Instance;

    [SerializeField] private TextMeshProUGUI moveDebugText;
    [SerializeField] private TextMeshProUGUI JumpAndGravityDebugText;
    [SerializeField] private TextMeshProUGUI GroundedCheckDebugText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (moveDebugText == null)
        {
            moveDebugText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void Log(string message)
    {
        if (moveDebugText != null)
        {
            moveDebugText.text = message;
        }

        Debug.Log(message); // 콘솔에도 찍기
    }
}
