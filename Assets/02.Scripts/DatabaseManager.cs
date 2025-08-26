using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    [SerializeField] GameUI m_gameUI;
    DatabaseReference m_databaseRef;
    string m_userId;

    [SerializeField] int m_maxGoldCount;
    //[SerializeField] SliderTimer m_slidertimer;

    [System.Serializable]
    public class PlayerData
    {
        public string Nickname;
        public int Gold;
        public int JoinOrder;

        public PlayerData(string nickname, int gold, int joinOrder)
        {
            Nickname = nickname;
            Gold = gold;
            JoinOrder = joinOrder;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialized();
        }
        else Destroy(Instance);
    }

    void Initialized()
    {
        m_userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        m_databaseRef = FirebaseDatabase.DefaultInstance.RootReference;

        //LoadGoldFromDatabase();
        //LoadNickFromDatabase();
    }

    public void LoadRoomList(System.Action<List<string>> onComplete)    //방 목록 불러오기
    {
        m_databaseRef.Child("rooms").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                List<string> roomIds = new List<string>();
                foreach (var child in task.Result.Children)
                {
                    roomIds.Add(child.Key); //roomId 추출
                }
                onComplete?.Invoke(roomIds);
            }
        });
    }

    public void JoinRoom(string roomId)
    {
        string nickName = "NoName";

        //우선 닉네임 가져오기
        m_databaseRef.Child("users").Child(m_userId).Child("Nickname").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists) nickName = task.Result.Value.ToString();

            m_databaseRef.Child("rooms").Child(roomId).Child("players").GetValueAsync().ContinueWithOnMainThread(playersTask =>
            {
                int joinOrder = 0;
                if (playersTask.IsCompletedSuccessfully) joinOrder = (int)playersTask.Result.ChildrenCount;

                PlayerData playerData = new PlayerData(nickName, 0, joinOrder);

                string json = JsonUtility.ToJson(playerData);

                //방 참가 시 내 데이터 등록
                m_databaseRef.Child("rooms").Child(roomId).Child("players").Child(m_userId).SetRawJsonValueAsync(json);
            });
        });
    }

    private int GetJoinOrder(string roomId)
    {
        //방에 이미 몇 명 있는지 세고 그 다음 숫자 반환
        int order = 0;
        m_databaseRef.Child("rooms").Child(roomId).Child("players").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully) order = (int)task.Result.ChildrenCount;
        });
        return order;
    }

    public void AssignJoinOrder()   //접속한 순서 받아오기
    {
        m_databaseRef.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                int currentCount = (int)task.Result.ChildrenCount;

                //자기 userId 밑에 JoinOrder 저장
                m_databaseRef.Child("users").Child(m_userId).Child("JoinOrder").SetValueAsync(currentCount - 1);
            }
        });
    }

    public void LoadGoldFromDatabase()                  //골드값 불러오기
    {
        m_databaseRef.Child("users").Child(m_userId).Child("Gold").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var snapshot = task.Result;
                if (snapshot.Exists)
                {
                    int gold = int.Parse(snapshot.Value.ToString());
                    m_gameUI.SetGold(gold);
                }
                else
                {
                    int defaultGoldCount = 0;
                    Debug.LogWarning("Gold 값이 데이터베이스에 없음.");
                    m_gameUI.SetGold(defaultGoldCount);
                }
            }
            else
                Debug.LogError("Gold 불러오기 실패: " + task.Exception);
        });
    }

    public void LoadNickFromDatabase()                  //닉네임 불러오기
    {
        m_databaseRef.Child("users").Child(m_userId).Child("Nickname").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string nickName = snapshot.Value.ToString();
                    m_gameUI.SetNickName(nickName);
                }
                else
                {
                    Debug.LogWarning("닉네임이 데이터베이스에 없음.");
                    m_gameUI.SetNickName("NoName");
                }
            }
            else
                Debug.LogError("닉네임 불러오기 실패: " + task.Exception);
        });
    }

    public void ChangeGold(string roomId, int amount)   //골드값 변경
    {
        int newGoldCount = Mathf.Clamp(m_gameUI.m_curGold + amount, 0, m_maxGoldCount);

        m_databaseRef.Child("rooms").Child(roomId).Child("players").Child(m_userId).Child("Gold").SetValueAsync(newGoldCount).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully) m_gameUI.SetGold(newGoldCount);
            else Debug.LogError("Gold 저장 실패: " + task.Exception);
        });
    }

    public void ResetRoomPlayersGold(string roomId)     //플레이어 골드 데이터 초기화
    {
        m_databaseRef.Child("rooms").Child(roomId).Child("players").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                foreach (var child in task.Result.Children)
                {
                    string userId = child.Key;
                    m_databaseRef.Child("rooms").Child(roomId).Child("players").Child(userId).Child("Gold").SetValueAsync(0);
                }
            }
        });
    }

    public void LoadRoomPlayers(string roomId)          //플레이어 데이터 불러오기
    {
        m_databaseRef.Child("rooms").Child(roomId).Child("players").OrderByChild("JoinOrder").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                int index = 0;
                foreach (var child in task.Result.Children)
                {
                    string nick = child.Child("Nickname").Value?.ToString() ?? "NoName";
                    int gold = int.Parse(child.Child("Gold").Value?.ToString() ?? "0");

                    m_gameUI.UpdatePlayerUI(index, nick, gold);
                    index++;
                }
            }
        });
    }
}
