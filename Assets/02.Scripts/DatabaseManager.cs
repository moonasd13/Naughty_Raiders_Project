using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System;
using System.Collections;

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

        LoadGoldFromDatabase();
        LoadNickFromDatabase();
    }

    public void LoadGoldFromDatabase()  //골드값 불러오기
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

    public void LoadNickFromDatabase()  //닉네임 불러오기
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

    public void ChangeGold(int amount)  //골드값 변경
    {
        int newGoldCount = Mathf.Clamp(m_gameUI.m_curGold + amount, 0, m_maxGoldCount);

        m_databaseRef.Child("users").Child(m_userId).Child("Gold").SetValueAsync(newGoldCount).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
                m_gameUI.SetGold(newGoldCount);
            else
                Debug.LogError("Gold 저장 실패: " + task.Exception);
        });
    }

    public void ResetAllPlayersGold()       //플레이어 골드 데이터 초기화
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
            }
        });
    }

    public void LoadAllPlayersData()        //플레이어 데이터 불러오기
    {
        m_databaseRef.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
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
