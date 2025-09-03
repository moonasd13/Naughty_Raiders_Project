using UnityEngine;
using UnityEngine.SceneManagement;

public class WidgetController : MonoBehaviour
{
    private const string TargetSceneName = "02_InGame_Test";
    public void SceneChange()
    {
        SceneManager.LoadScene(TargetSceneName);
    }
    public void RoomJoin()
    {
        Debug.Log("¹æ»ý¼º");
    }
}
