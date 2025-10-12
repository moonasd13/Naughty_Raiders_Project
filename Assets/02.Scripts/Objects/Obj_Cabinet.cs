using StarterAssets;
using Unity.Netcode;
using UnityEngine;

public class Obj_Cabinet : NetworkBehaviour
{
    [SerializeField] private Transform showPosition; // ³ª¿Ã À§Ä¡
    [SerializeField] private Collider _senseZone;

    public void Interact(PlayerMove player)
    {
        if (!IsServer)
        {
            RequestInteractServerRpc(player.OwnerClientId);
            return;
        }

        if (!player.hide.Value)
            HidePlayer(player);
        else
            ShowPlayer(player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInteractServerRpc(ulong clientId)
    {
        PlayerMove player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerMove>();
        if (player != null)
        {
            if (!player.hide.Value)
                HidePlayer(player);
            else
                ShowPlayer(player);
        }
    }

    private void HidePlayer(PlayerMove player)
    {
        player.hide.Value = true;
        TogglePlayerVisibilityClientRpc(player.OwnerClientId, false, Vector3.zero, Quaternion.identity);
        Debug.Log($"Player {player.OwnerClientId} ¼û±è Ã³¸® ¿Ï·á");
    }

    private void ShowPlayer(PlayerMove player)
    {
        player.hide.Value = false;
        TogglePlayerVisibilityClientRpc(player.OwnerClientId, true, showPosition.position, showPosition.rotation);
        Debug.Log($"Player {player.OwnerClientId} ¼û±è ÇØÁ¦ ¿Ï·á");
    }

    [ClientRpc]
    private void TogglePlayerVisibilityClientRpc(ulong clientId, bool visible, Vector3 pos, Quaternion rot)
    {
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
            return;

        PlayerMove player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerMove>();
        if (player == null) return;

        foreach (var renderer in player.GetComponentsInChildren<Renderer>())
            renderer.enabled = visible;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = visible;

        Collider[] colliders = player.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = visible;

        if (visible && pos != Vector3.zero)
        {
            if (cc != null) cc.enabled = false;
            player.transform.SetPositionAndRotation(pos, rot);
            if (cc != null) cc.enabled = true;
        }
    }
}
