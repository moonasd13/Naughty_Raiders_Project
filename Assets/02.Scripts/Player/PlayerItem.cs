using Define_Enums;
using StarterAssets;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerItem : NetworkBehaviour
{
    public void Useitem(PlayerMove playerMove)
    {
        UseItemServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = true)]
    public void UseItemServerRpc(ulong clientId)
    {
        var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        var playerController = playerObject.GetComponent<PlayerMove>();

        switch (playerController.my_ItemKind.Value)
        {
            case ItemKind.Gun:
                playerController.UseGun();
                break;

            case ItemKind.Speed:
                playerController.CjangeSpeed();
                break;
        }

        playerController.equip.Value = false;
        playerController.my_ItemKind.Value = ItemKind.None;

        var playerData = playerController.GetComponent<PlayerData>();
        playerData.CurrentItem.Value = (int)ItemKind.None;
    }
}

