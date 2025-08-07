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

    // 현재 범위 안에 있는 플레이어 목록
    private readonly Dictionary<ulong, PlayerController> playersInZone = new();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null && !playersInZone.ContainsKey(controller.OwnerClientId))
            {
                playersInZone.Add(controller.OwnerClientId, controller);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null && playersInZone.ContainsKey(controller.OwnerClientId))
            {
                playersInZone.Remove(controller.OwnerClientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitInteractServerRpc(ulong clientId)
    {
        if (!playersInZone.ContainsKey(clientId)) return;

        PlayerController controller = playersInZone[clientId];

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
        }
    }

    [ClientRpc]
    private void SendGoldToClientRpc(ulong clientId, int goldAmount)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            DatabaseManager.Instance.ChangeGold(goldAmount);
        }
    }
}
