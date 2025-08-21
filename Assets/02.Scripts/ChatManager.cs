using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public InputField inputField;
    public Button enterButton;
    public Transform contentTransform;
    public GameObject chatTextPrefab;
    public ScrollRect scrollrect;

    private DatabaseReference dbRef;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("chats").RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("기존 채팅 로그 초기화 완료");
            }
            else
            {
                Debug.LogError("채팅 초기화 실패: " + task.Exception);
            }
        });

        enterButton.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(inputField.text))
            {
                string nickname = AuthManager.Instance.CurrentNickname ?? "NoName";
                SendMessageToDB(nickname, inputField.text);
                inputField.text = "";
            }
        });

        StartListening();
    }

    void SendMessageToDB(string userId, string message)     //메세지 저장
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string key = dbRef.Child("chats").Push().Key;

        var data = new Dictionary<string, object>()
        {
            { "userId", userId },
            { "message", message },
            { "timestamp", timestamp }
        };

        dbRef.Child("chats").Child(key).SetValueAsync(data);
    }

    void StartListening()
    {
        FirebaseDatabase.DefaultInstance.GetReference("chats").OrderByChild("timestamp").LimitToLast(50).ValueChanged += (sender, args) =>
        {
            foreach (Transform child in contentTransform)
            {
                Destroy(child.gameObject); //기존 메시지 삭제 후 재로딩
            }

            foreach (var msg in args.Snapshot.Children)
            {
                string userId = msg.Child("userId").Value.ToString();
                string message = msg.Child("message").Value.ToString();

                GameObject textObj = Instantiate(chatTextPrefab, contentTransform);
                textObj.GetComponent<Text>().text = $"{userId}: {message}";
            }

            Canvas.ForceUpdateCanvases();
            scrollrect.verticalNormalizedPosition = 0f; //스크롤을 맨 아래로 이동
        };
    }
}
