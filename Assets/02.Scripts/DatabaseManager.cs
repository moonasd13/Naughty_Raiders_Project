using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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

    public void LoadGoldFromDatabase(PlayerData player, string firebaseUid)
    {
        m_databaseRef.Child("users").Child(firebaseUid).Child("Gold").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                int gold = 0;
                var snapshot = task.Result;
                if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out gold))
                {
                    player.SetGoldFromDatabaseServerRpc(gold);
                }
                else
                {
                    player.SetGoldFromDatabaseServerRpc(0);
                }
            }
        });
    }

    public void LoadNickFromDatabase(PlayerData player, string firebaseUid)
    {
        m_databaseRef.Child("users").Child(firebaseUid).Child("Nickname").GetValueAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var snapshot = task.Result;
                string nick = snapshot.Exists ? snapshot.Value.ToString() : "NoName";
                player.SetNicknameFromDatabaseServerRpc(nick);
            }
        });
    }

    public void ChangeGold(PlayerData player, int amount, string firebaseUid)
    {
        int newGoldCount = Mathf.Clamp(player.Gold.Value + amount, 0, m_maxGoldCount);

        m_databaseRef.Child("users").Child(firebaseUid).Child("Gold").SetValueAsync(newGoldCount)
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                player.SetGoldFromDatabaseServerRpc(newGoldCount); // 서버에서 갱신
            }
        });
    }

    public void ResetAllPlayersGold()
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
                    player.ResetGoldServerRpc();
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
        });
    }
}
