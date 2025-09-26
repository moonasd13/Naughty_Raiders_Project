using StarterAssets;
using Unity.Netcode;
using UnityEngine;

public class Obj_Cabinet : NetworkBehaviour
{
    [SerializeField] private Transform hidePosition; // 인스펙터에서 지정
    [SerializeField] private Transform showPosition;
    [SerializeField] private Collider _senseZone;

    public void Interact(PlayerMove player)
    {
        if (!IsServer)
        {
            RequestInteractServerRpc(player.OwnerClientId);
            return;
        }

        if (player.hide.Value == false)
        {
            HidePlayer(player);
        }
        else
        {
            ShowPlayer(player);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInteractServerRpc(ulong clientId)
    {
        PlayerMove player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerMove>();
        if (player != null)
        {
            if (player.hide.Value == false)
            {
                HidePlayer(player);
            }
            else
            {
                ShowPlayer(player);
            }
        }
    }

    private void HidePlayer(PlayerMove player)
    {

        //상태값 동기화용 NetworkVariable 사용 가능
        player.hide.Value = true;

        //플레이어 위치 이동
        HidePlayerClientRpc(player.OwnerClientId, hidePosition.position, hidePosition.rotation);

        Debug.Log($"Player {player.OwnerClientId} 숨김 처리 완료");
    }

    private void ShowPlayer(PlayerMove player)
    {

        player.hide.Value = false;
        HidePlayerClientRpc(player.OwnerClientId, showPosition.position, showPosition.rotation);

        Debug.Log($"Player {player.OwnerClientId} 숨김 처리 해제 완료");
    }


    [ClientRpc]
    private void HidePlayerClientRpc(ulong clientId, Vector3 pos, Quaternion rot)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        // 플레이어 오브젝트 가져오기
        PlayerMove player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerMove>();
        if (player == null) return;

        var cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = pos;
            player.transform.rotation = rot;
            cc.enabled = true;
        }
        else
        {
            player.transform.position = pos;
            player.transform.rotation = rot;
        }
    }


}
