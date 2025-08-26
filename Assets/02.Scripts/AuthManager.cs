using Firebase.Auth;
using Firebase.Extensions;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Firebase.Database;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    DatabaseReference m_dataBaseRef;
    FirebaseAuth m_auth;

    [SerializeField]
    MainSceneUI m_MainUI;

    public string CurrentNickname { get; private set; }

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
            Destroy(gameObject);
    }
    #endregion

    #region Initialized
    void Initialized()
    {
        m_auth = FirebaseAuth.DefaultInstance;
        m_dataBaseRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void InitUserData(string uid)
    {
        m_dataBaseRef.Child("users").Child(uid).Child("Gold").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Gold 확인 중 오류: " + task.Exception);

                return;
            }

            if (!task.Result.Exists)
            {
                m_dataBaseRef.Child("users").Child(uid).Child("Gold").SetValueAsync(0).ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsCompletedSuccessfully)
                        Debug.Log("Gold 기본값 0으로 설정됨");
                    else
                        Debug.LogError("Gold 설정 실패: " + setTask.Exception);
                });
            }
        });
    }
    #endregion

    #region Login & SignUp
    public void Login(string email, string password)
    {
        m_auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                FirebaseUser user = task.Result.User;
                Debug.Log("로그인 성공: " + user.UserId);
                InitUserData(user.UserId);

                LoginSuccess();

                LoadScene();
            }
            else
            {
                Debug.Log("로그인 실패: " + task.Exception);
            }
        });
    }

    public void SignUp(string email, string password)
    {
        m_auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                LoginSuccess();
                Debug.Log("회원가입 실패: " + task.Exception);
                return;
            }
            else
            {
                FirebaseUser newUser = task.Result.User;
                SaveNickname(m_MainUI.m_nickNameField);
                Debug.Log("회원가입 성공: " + newUser.UserId);
            }
        });
    }
    #endregion

    #region NickName
    public void LoginSuccess()
    {
        string userId = m_auth.CurrentUser.UserId;

        m_dataBaseRef.Child("users").Child(userId).Child("Nickname").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if(task.IsCompletedSuccessfully)
            {
                if(task.Result.Exists)
                {
                    string nickname = task.Result.Value.ToString();
                    CurrentNickname = nickname;
                    Debug.Log("닉네임 존재함: " + nickname);
                }
                else
                {
                    Debug.Log("닉네임 설정 필요.");
                }
            }
            else
            {
                Debug.LogError("닉네임 확인 실패: " + task.Exception);
            }
        });
    }

    public void SaveNickname(InputField nicknameField)
    {
        string userId = m_auth.CurrentUser.UserId;
        string nickname = nicknameField.text.Trim();
       
        if(!string.IsNullOrEmpty(nickname))
        {
            m_dataBaseRef.Child("users").Child(userId).Child("Nickname").SetValueAsync(nickname).ContinueWithOnMainThread(task => 
            {
                if (task.IsCompletedSuccessfully)
                {
                    CurrentNickname = nickname;
                    Debug.Log("닉네임 저장");
                }
                else
                    Debug.Log("닉네임 저장 실패" + task.Exception);
            });
        }
    }
    #endregion

    #region LoadScene
    void LoadScene()
    {
        SceneManager.LoadSceneAsync("01_Lobby_Test");
    }
    #endregion
}
