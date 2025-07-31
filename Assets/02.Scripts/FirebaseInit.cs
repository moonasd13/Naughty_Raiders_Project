using Firebase.Database;
using Firebase.Extensions;
using Firebase;
using UnityEngine;
using System;

public class FirebaseInit : MonoBehaviour
{
    public static FirebaseInit Instance { get; private set; }
    public DatabaseReference m_dataBaseRef { get; private set; }

    [Header("DataBase URL")]
    [SerializeField]
    string m_databaseURL = "https://naeunfirebasetest-default-rtdb.firebaseio.com/";

    #region UnityMethod
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialized();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Initialized
    void Initialized()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                app.Options.DatabaseUrl = new Uri(m_databaseURL);
                m_dataBaseRef = FirebaseDatabase.GetInstance(app).RootReference;
                Debug.Log("초기화 성공");
            }
            else
                Debug.Log("초기화 실패");
        });
    }
    #endregion
}
