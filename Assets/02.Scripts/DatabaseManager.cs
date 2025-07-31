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
}
