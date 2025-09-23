using UnityEngine;
using Unity.Netcode;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private Transform playerListParent;
    [SerializeField] private LobbyRoomPlayerInfoUI playerInfoPrefab;

    private void RefreshPlayerList()
    {
        // 기존 UI 삭제
        foreach (Transform child in playerListParent)
            Destroy(child.gameObject);

        // 현재 접속 중인 모든 클라이언트 순회
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;
            if (playerObj == null) continue;
            
            var playerNameUI = playerObj.GetComponent<PlayerNameUI>();
            if (playerNameUI == null) continue;

            var ui = Instantiate(playerInfoPrefab, playerListParent);
            ui.SetPlayer(playerNameUI);
        }
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (_) => RefreshPlayerList();
        NetworkManager.Singleton.OnClientDisconnectCallback += (_) => RefreshPlayerList();
    }
}
