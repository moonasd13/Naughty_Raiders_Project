using UnityEngine;
using UnityEngine.SceneManagement;

public class WidgetController : MonoBehaviour
{
    private const string TargetSceneName = "NetCodeTestScene2";
    public void SceneChange()
    {
        SceneManager.LoadScene(TargetSceneName);
    }
}
