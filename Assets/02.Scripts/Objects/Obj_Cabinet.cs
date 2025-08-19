using StarterAssets;
using Unity.Netcode;
using UnityEngine;

public class Obj_Cabinet : NetworkBehaviour
{
    [SerializeField] private Transform hidePosition; // 인스펙터에서 지정
    [SerializeField] private Collider _senseZone;

    public void Interact(PlayerController player)
    {
        if (!IsServer)
        {
            RequestInteractServerRpc(player.OwnerClientId);
            return;
        }

        HidePlayer(player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInteractServerRpc(ulong clientId)
    {
        PlayerController player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerController>();
        if (player != null)
        {
            HidePlayer(player);
        }
    }

    private void HidePlayer(PlayerController player)
    {
        // 플레이어 위치 이동
        player.transform.position = hidePosition.position;
        player.transform.rotation = hidePosition.rotation;

        // 플레이어 비활성화 (렌더러 끄기)
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.enabled = false;
        }

        // 상태값 동기화용 NetworkVariable 사용 가능
        player.hide.Value = true;

        Debug.Log($"Player {player.OwnerClientId} 숨김 처리 완료");
    }





}
