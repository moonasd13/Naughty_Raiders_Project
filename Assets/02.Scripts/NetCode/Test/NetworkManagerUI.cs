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
        serverBtn.AddEvent(() => NetworkManager.Singleton.StartServer());
        hostBtn.AddEvent(() => NetworkManager.Singleton.StartHost());
        clientBtn.AddEvent(() => NetworkManager.Singleton.StartClient());
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