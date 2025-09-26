using StarterAssets;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoomItemBox : NetworkBehaviour
{
    [SerializeField]
    public BoxCollider RoomArea;
    public GameObject ScoreObject;

    [Header("정보")]
    public NetworkVariable<int> boxCount = new NetworkVariable<int>(0);
    [SerializeField] private int rewardPerBox = 10;

    [ServerRpc(RequireOwnership = false)]
    public void SubmitInteractServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client);
        PlayerMove controller = client.PlayerObject.GetComponent<PlayerMove>();

        if (controller == null) return;


        if (controller.inHand.Value)
        {
            controller.inHand.Value = false;
            boxCount.Value++;
            SendGoldToClientRpc(clientId, rewardPerBox);
        }
        else if (!controller.inHand.Value && boxCount.Value > 0)
        {
            controller.inHand.Value = true;
            boxCount.Value--;
            SendGoldToClientRpc(clientId, -rewardPerBox);
        }
    }

    [ClientRpc]
    private void SendGoldToClientRpc(ulong clientId, int goldAmount)
    {
        // 이 클라이언트가 대상인지 확인
        if (NetworkManager.LocalClientId != clientId) return;

        // PlayerData 가져오기
        var localPlayerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayerObj == null)
        {
            Debug.LogWarning("SendGoldToClientRpc: Local PlayerObject가 null입니다.");
            return;
        }

        var playerData = localPlayerObj.GetComponent<PlayerData>();
        if (playerData == null)
        {
            Debug.LogWarning("SendGoldToClientRpc: PlayerData가 null입니다.");
            return;
        }

        // DatabaseManager 호출 (Firebase UID 필요)
        if (DatabaseManager.Instance != null)
        {
            string uid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
            if (!string.IsNullOrEmpty(uid))
            {
                DatabaseManager.Instance.ChangeGold(playerData, goldAmount, uid);
            }
            else
            {
                Debug.LogWarning("SendGoldToClientRpc: Firebase UID가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("SendGoldToClientRpc: DatabaseManager.Instance가 null입니다.");
        }
    }

}
