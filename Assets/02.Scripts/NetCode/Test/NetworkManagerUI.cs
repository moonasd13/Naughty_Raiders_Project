using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] Button serverBtn;
    [SerializeField] Button hostBtn;
    [SerializeField] Button clientBtn;

    void Awake()
    {
        serverBtn.AddEvent(() =>
        {
            NetworkManager.Singleton.StartServer();
            Debug.Log("서버로 시작했습니다.");
        });

        hostBtn.AddEvent(() =>
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("호스트로 시작했습니다.");
        });

        clientBtn.AddEvent(() =>
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("클라이언트로 접속을 시도했습니다.");
        });
    }
}

public static class Extension
{
    public static void AddEvent(this Button btn, UnityAction action)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }
}
