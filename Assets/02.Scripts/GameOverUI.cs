using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button goToSceneButton;

    private void Awake()
    {
        Instance = this;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        // 버튼 이벤트 등록
        quitButton.onClick.AddListener(QuitGame);
        goToSceneButton.onClick.AddListener(GoToScene);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel == null)
        {
            Debug.LogError("GameOverPanel이 연결되지 않음!");
            return;
        }
        gameOverPanel.SetActive(true);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 종료
#else
        Application.Quit(); // 빌드된 게임에서 종료
#endif
    }

    private void GoToScene()
    {
        SceneManager.LoadScene("01_Lobby");
    }
}
