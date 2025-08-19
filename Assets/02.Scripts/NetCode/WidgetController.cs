using UnityEngine;
using UnityEngine.SceneManagement;

public class WidgetController : MonoBehaviour
{
    private const string TargetSceneName = "02_InGame";
    public void SceneChange()
    {
        SceneManager.LoadScene(TargetSceneName);
    }
}
