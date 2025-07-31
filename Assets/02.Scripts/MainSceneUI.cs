using UnityEngine;
using UnityEngine.UI;

public class MainSceneUI : MonoBehaviour
{
    [Header("Field")]
    [SerializeField]
    InputField m_emailField;
    [SerializeField]
    InputField m_passwordField;

    public InputField m_nickNameField;

    [Header("AuthManager")]
    [SerializeField]
    AuthManager m_authManager;

    void Start()
    {
        Initialized();
    }

    void Initialized()
    {
        
    }

    #region Login & SignUp Button_Event
    public void Login()
    {
        m_authManager.Login(m_emailField.text, m_passwordField.text);
    }

    public void SignUp()
    {
        m_authManager.SignUp(m_emailField.text, m_passwordField.text);
    }
    #endregion
}
