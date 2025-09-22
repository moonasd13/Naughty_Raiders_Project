using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameTimerUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider timerSlider;

    private GameTimer timer;
    private float maxTime;

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

        maxTime = timer.MaxTime;

        if (timerSlider != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = 1f;
            timerSlider.value = 1f;     //시작 시 꽉 찬 상태
        }

        //값 변경 시 즉시 갱신되도록(프레임마다 폴링도 하긴 함)
        timer.TimeLeft.OnValueChanged += (_, __) => UpdateUI();
        timer.IsRunning.OnValueChanged += (_, __) => UpdateUI();

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (timer != null)
        {
            timer.TimeLeft.OnValueChanged -= (_, __) => UpdateUI(); ;
            timer.IsRunning.OnValueChanged -= (_, __) => UpdateUI();
        }
    }

    private void LateUpdate()
    {
        // 부드럽게 갱신되도록 프레임마다 업데이트(특히 로컬 호스트 UI)
        if (timer != null)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (timer == null || timerText == null) return;

        float t = Mathf.Max(0f, timer.TimeLeft.Value);
        int totalSeconds = Mathf.CeilToInt(t);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (minutes >= 1)
        {
            //분이 10 미만이면 앞자리 0 빼고 표시
            timerText.text = $"{minutes}:{seconds:00}";
        }
        else
        {
            //1분 미만 → 초만 출력
            timerText.text = $"{seconds}";
        }

        if (timerSlider != null) timerSlider.value = t / maxTime;
    }
}
