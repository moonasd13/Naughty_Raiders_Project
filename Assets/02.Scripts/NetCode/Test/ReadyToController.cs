using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using DefineEnum;

public class ReadyToController : NetworkBehaviour
{
    public static Dictionary<ulong, bool> playerReady = new Dictionary<ulong, bool>();

    private void Start()
    {
        if (IsServer)
        {
            // 호스트는 무조건 ready
            ulong hostId = NetworkManager.Singleton.LocalClientId;
            playerReady[hostId] = true;
            Debug.Log($"Host({hostId}) Ready = true");
        }
    }

    private void Update()
    {
        if (!IsOwner) return; // 자기 오브젝트만 입력 처리
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (IsServer)
            {
                // 호스트라면 직접 토글
                ToggleReady(NetworkManager.Singleton.LocalClientId);
            }
            else
            {
                // 클라는 서버Rpc 호출
                ToggleReadyServerRpc();
            }
        }

        // 호스트만 시작 체크 가능
        if (IsServer && Input.GetKeyDown(KeyCode.F6))
        {
            TryStartGame();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        ToggleReady(clientId);
    }

    private void ToggleReady(ulong clientId)
    {
        if (!playerReady.ContainsKey(clientId))
            playerReady[clientId] = true;
        else
            playerReady[clientId] = !playerReady[clientId];

        Debug.Log($"Client {clientId} Ready = {playerReady[clientId]}");
    }

    private void TryStartGame()
    {
        foreach (var kvp in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReady.ContainsKey(kvp) || !playerReady[kvp])
            {
                Debug.Log($"아직 준비 안 된 플레이어 있음! (ClientId={kvp})");
                return;
            }
        }

        Debug.Log("모든 플레이어 준비 완료! 게임 시작!");
        GameManger.Instance.GameStateChange(GameState.firstTime);
    }
}