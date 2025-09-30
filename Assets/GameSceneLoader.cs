using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLoader : MonoBehaviour
{
    public static GameSceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void LoadGameScene(string roomId)
    {
        //게임 씬 로드
        SceneManager.LoadScene("NetCodeTestScene2");

        //씬 로드 후 DatabaseManager에 RoomId 전달
        StartCoroutine(SetRoomIdNextFrame(roomId));
    }

    private IEnumerator SetRoomIdNextFrame(string roomId)
    {
        //씬 로드가 끝난 다음 프레임까지 대기
        yield return null;

        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.SetCurrentRoomId(roomId);
            Debug.Log($"RoomId 전달 완료: {roomId}");
        }
        else
        {
            Debug.LogError("DatabaseManager가 존재하지 않습니다!");
        }
    }
}
