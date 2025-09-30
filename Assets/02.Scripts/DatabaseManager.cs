using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
<<<<<<< Updated upstream
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
=======
using UnityEngine;
>>>>>>> Stashed changes

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    [SerializeField] GameUI1 m_gameUI;  // 게임 씬 UI
    DatabaseReference m_databaseRef;
    string m_userId;

    [SerializeField] int m_maxGoldCount;

    private string m_currentRoomId;
    public string CurrentRoomId => m_currentRoomId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            m_userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            m_databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        }
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (m_gameUI == null) m_gameUI = FindObjectOfType<GameUI1>();

<<<<<<< Updated upstream
        //LoadGoldFromDatabase();
        //LoadNickFromDatabase();
    }

    public void LoadGoldFromDatabase(PlayerData player, string firebaseUid)     //골드 불러오기
    {
        m_databaseRef.Child("users").Child(firebaseUid).Child("Gold").GetValueAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                int gold = 0;
                var snapshot = task.Result;
                if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out gold))
                {
                    player.Gold.Value = gold;  //UI 말고 PlayerData에 반영
                }
                else
                {
                    player.Gold.Value = 0;
                }
            }
        });
    }

    public void LoadNickFromDatabase(PlayerData player, string firebaseUid)     //닉네임 불러오기
    {
        m_databaseRef.Child("users").Child(firebaseUid).Child("Nickname").GetValueAsync()
        .ContinueWithOnMainThread(task =>
=======
        if (!string.IsNullOrEmpty(CurrentRoomId)) LoadAllPlayersData();
    }

    public void SetCurrentRoomId(string roomId)
    {
        m_currentRoomId = roomId;
    }

    public void LoadAllPlayersData()
    {
        if (string.IsNullOrEmpty(m_currentRoomId))
>>>>>>> Stashed changes
        {
            Debug.LogError("RoomId가 설정되지 않았습니다!");
            return;
        }

        RoomManager.Instance.GetPlayersInRoom(m_currentRoomId, playerIds =>
        {
            int index = 0;
            foreach (var uid in playerIds)
            {
<<<<<<< Updated upstream
                var snapshot = task.Result;
                player.Nickname.Value = snapshot.Exists ? snapshot.Value.ToString() : "NoName";
=======
                if (index >= m_gameUI.playersUI.Count) break;
                m_gameUI.SetPlayerUI(index, uid);

                // 닉네임 불러오기
                m_databaseRef.Child("users").Child(uid).Child("Nickname").GetValueAsync().ContinueWithOnMainThread(nickTask =>
                {
                    string nick = nickTask.Result.Exists ? nickTask.Result.Value.ToString() : "NoName";

                    // 골드 불러오기
                    m_databaseRef.Child("users").Child(uid).Child("Gold").GetValueAsync().ContinueWithOnMainThread(goldTask =>
                    {
                        int gold = goldTask.Result.Exists ? int.Parse(goldTask.Result.Value.ToString()) : 0;

                        // GameUI 갱신
                        m_gameUI.UpdatePlayerData(uid, nick, gold);
                    });
                });
                index++;
>>>>>>> Stashed changes
            }
        });
    }

<<<<<<< Updated upstream
    public void ChangeGold(PlayerData player, int amount, string firebaseUid)
    {
        int newGoldCount = Mathf.Clamp(player.Gold.Value + amount, 0, m_maxGoldCount);

        m_databaseRef.Child("users").Child(firebaseUid).Child("Gold").SetValueAsync(newGoldCount)
        .ContinueWithOnMainThread(task =>
=======
    public void ResetAllPlayersGold()
    {
        if (string.IsNullOrEmpty(m_currentRoomId))
>>>>>>> Stashed changes
        {
            Debug.LogError("RoomId가 설정되지 않았습니다!");
            return;
        }

        RoomManager.Instance.GetPlayersInRoom(m_currentRoomId, playerIds =>
        {
            foreach (var uid in playerIds)
            {
<<<<<<< Updated upstream
                player.Gold.Value = newGoldCount; // 네트워크 변수 갱신
            }
        });
    }

    public void ResetAllPlayersGold()     //플레이어 골드 데이터 초기화
    {
        m_databaseRef.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                foreach (var child in task.Result.Children)
                {
                    string userId = child.Key;
                    m_databaseRef.Child("users").Child(userId).Child("Gold").SetValueAsync(0);
                }

                foreach (var player in FindObjectsByType<PlayerData>(FindObjectsSortMode.None))
                {
                    if (player.IsServer)
                        player.Gold.Value = 0;
                }
            }
        });
    }

    public void SaveGoldToDatabase(string uid, int gold)
    {
        m_databaseRef.Child("users").Child(uid).Child("Gold").SetValueAsync(gold).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log($"골드 {gold} 저장 완료: {uid}");
            else
                Debug.LogError("골드 저장 실패: " + task.Exception);
=======
                m_databaseRef.Child("users").Child(uid).Child("Gold").SetValueAsync(0).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        m_gameUI.UpdatePlayerData(uid, null, 0);
                    }
                });
            }
>>>>>>> Stashed changes
        });
    }
}
