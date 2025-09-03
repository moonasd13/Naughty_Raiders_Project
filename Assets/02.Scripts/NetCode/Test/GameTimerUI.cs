using TMPro;
using UnityEngine;

public class GameTimerUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private TextMeshProUGUI timerText;

    private GameTimer timer;

    private void Start()
    {
        // 씬에 GameTimer 프리팹/오브젝트가 있어야 함
        timer = GameTimer.Instance;

        if (timer == null)
        {
            Debug.LogWarning("GameTimer 인스턴스를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        // 값 변경 시 즉시 갱신되도록(프레임마다 폴링도 하긴 함)
        timer.TimeLeft.OnValueChanged += (_, __) => UpdateText();
        timer.IsRunning.OnValueChanged += (_, __) => UpdateText();

        UpdateText();
    }

    private void OnDestroy()
    {
        if (timer != null)
        {
            timer.TimeLeft.OnValueChanged -= (_, __) => UpdateText();
            timer.IsRunning.OnValueChanged -= (_, __) => UpdateText();
        }
    }

    private void LateUpdate()
    {
        // 부드럽게 갱신되도록 프레임마다 업데이트(특히 로컬 호스트 UI)
        if (timer != null) UpdateText();
    }

    private void UpdateText()
    {
        if (timer == null || timerText == null) return;

        float t = Mathf.Max(0f, timer.TimeLeft.Value);
        int totalSeconds = Mathf.CeilToInt(t);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
