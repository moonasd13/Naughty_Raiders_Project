using StarterAssets;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class Obj_Cabinet : NetworkBehaviour
{
    [SerializeField] private Transform hidePosition; // 인스펙터에서 지정
    [SerializeField] private Transform showPosition;
    [SerializeField] private Collider _senseZone;

    private bool In_hide = false;


    public void Interact(PlayerController player)
    {
        if (In_hide)
            Debug.Log("숨어있음");
            return;

        if (!IsServer)
        {
            RequestInteractServerRpc(player.NetworkObject);
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
    private void RequestInteractServerRpc(NetworkObjectReference playerRef)
    {
        Debug.Log("RPC들어옴");

        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            // 여기서는 playerObj 그대로 사용
            PlayerController player = playerObj.GetComponent<PlayerController>();
            if (player != null)
            {
                if (!player.hide.Value)
                    HidePlayer(player);
                else
                    ShowPlayer(player);
            }

            Debug.Log($"PlayerObject: {playerObj}, Has PlayerController: {player != null}");
        }
        else
        {
            Debug.LogWarning("ServerRpc: PlayerObj를 찾지 못함!");
        }

    }

    private void HidePlayer(PlayerController player)
    {
        // 플레이어 위치 이동
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.SetPositionAndRotation(hidePosition.position, hidePosition.rotation);
        player.GetComponent<CharacterController>().enabled = true;

        player.hide.Value = true;
        In_hide = true;

    }

    private void ShowPlayer(PlayerController player)
    {
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.SetPositionAndRotation(showPosition.position, showPosition.rotation);
        player.GetComponent<CharacterController>().enabled = true;

        player.hide.Value = false;
        In_hide = false;
    }
}
