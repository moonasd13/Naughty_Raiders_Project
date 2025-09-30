using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using TMPro;

public class RoomUI : MonoBehaviour
{
    [SerializeField] TMP_InputField m_roomIdInput;  // 방 번호 입력창
    [SerializeField] Button m_createBtn;            // 방 생성 버튼
    [SerializeField] Button m_joinBtn;              // 방 참가 버튼

    string m_userId;

    private void Start()
    {
        m_userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        m_createBtn.onClick.AddListener(OnClickCreateRoom);
        m_joinBtn.onClick.AddListener(OnClickJoinRoom);
    }

    void OnClickCreateRoom()
    {
        string roomId = m_roomIdInput.text.Trim();

        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogWarning("방 번호를 입력하세요!");
            return;
        }

        RoomManager.Instance.CreateRoom(roomId, m_userId);
        Debug.Log($"방 {roomId} 생성 요청");

        GameSceneLoader.Instance.LoadGameScene(roomId);
    }

    void OnClickJoinRoom()
    {
        string roomId = m_roomIdInput.text.Trim();

        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogWarning("방 번호를 입력하세요!");
            return;
        }

        RoomManager.Instance.JoinRoom(roomId, m_userId);
        Debug.Log($"방 {roomId} 참가 요청");
        //씬 전환 시 RoomId 전달
        GameSceneLoader.Instance.LoadGameScene(roomId);
    }

    public string GetRoomId()
    {
        if (m_roomIdInput != null)
        {
            return m_roomIdInput.text.Trim();
        }
        return string.Empty;
    }
}
