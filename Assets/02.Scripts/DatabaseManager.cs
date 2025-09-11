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

    public void LoadGoldFromDatabase(PlayerData player)     //골드 불러오기
    {
        m_databaseRef.Child("users").Child(m_userId).Child("Gold").GetValueAsync()
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

    public void LoadNickFromDatabase(PlayerData player)     //닉네임 불러오기
    {
        m_databaseRef.Child("users").Child(m_userId).Child("Nickname").GetValueAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var snapshot = task.Result;
                player.Nickname.Value = snapshot.Exists ? snapshot.Value.ToString() : "NoName";
            }
        });
    }

    public void ChangeGold(PlayerData player, int amount)
    {
        int newGoldCount = Mathf.Clamp(player.Gold.Value + amount, 0, m_maxGoldCount);

        m_databaseRef.Child("users").Child(m_userId).Child("Gold").SetValueAsync(newGoldCount)
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
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
}
