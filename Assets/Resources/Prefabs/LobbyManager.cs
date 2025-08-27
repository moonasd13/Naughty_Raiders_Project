using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] TMP_InputField roomInputField;
    [SerializeField] Transform roomListParent;      //ScrollView Content
    [SerializeField] GameObject roomButtonPrefab;   //방 버튼 프리팹

    public void OnClickCreateSession()
    {
        string roomId = roomInputField.text;

        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogWarning("방 이름을 입력하세요!");
            return;
        }

        DatabaseManager.Instance.JoinRoom(roomId);

        PlayerPrefs.SetString("CurrentRoomId", roomId);
        SceneManager.LoadScene("02_InGame");
    }

    public void RefreshRoomList()
    {
        //기존 리스트 초기화
        foreach (Transform child in roomListParent) Destroy(child.gameObject);

        DatabaseManager.Instance.LoadRoomList((roomIds) =>
        {
            foreach (string roomId in roomIds)
            {
                GameObject btnObj = Instantiate(roomButtonPrefab, roomListParent);

                var textComp = btnObj.GetComponentInChildren<TMP_Text>();
                if (textComp != null) textComp.text = roomId;

                btnObj.GetComponent<Button>().onClick.AddListener(() => OnClickJoinSession(roomId));
            }
        });
    }

    public void OnClickJoinSession(string roomId)
    {
        DatabaseManager.Instance.JoinRoom(roomId);
        PlayerPrefs.SetString("CurrentRoomId", roomId);
        SceneManager.LoadScene("02_InGame");
    }
}

