using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    DatabaseReference m_databaseRef;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            m_databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }
        else Destroy(gameObject);
    }

    public void CreateRoom(string roomId, string userId)        //방 생성 및 플레이어 추가
    {
        var roomRef = m_databaseRef.Child("rooms").Child(roomId);
        roomRef.Child("players").RemoveValueAsync();
        roomRef.Child("players").Child(userId).SetValueAsync(true);
        roomRef.Child("state").SetValueAsync("waiting");
    }

    public void JoinRoom(string roomId, string userId)          //방에 플레이어 추가
    {
        var roomRef = m_databaseRef.Child("rooms").Child(roomId);
        roomRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                roomRef.Child("players").Child(userId).SetValueAsync(true);
            }
            else
            {
                Debug.LogWarning("해당 방이 존재하지 않습니다.");
            }
        });
    }

    public void GetPlayersInRoom(string roomId, Action<List<string>> onResult)  //방에 있는 플레이어 목록 가져오기
    {
        var roomRef = m_databaseRef.Child("rooms").Child(roomId).Child("players");
        roomRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            List<string> playerIds = new List<string>();
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                foreach (var child in task.Result.Children)
                    playerIds.Add(child.Key);
            }
            onResult?.Invoke(playerIds);
        });
    }

    public void ResetAllPlayersGold(string roomId)      //방에 있는 모든 플레이어의 골드값을 0으로 초기화
    {
        GetPlayersInRoom(roomId, (playerIds) =>
        {
            foreach (var uid in playerIds)
            {
                m_databaseRef.Child("users").Child(uid).Child("Gold").SetValueAsync(0);
            }
        });
    }
}
