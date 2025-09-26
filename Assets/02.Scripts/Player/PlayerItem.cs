using Define_Enums;
using StarterAssets;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerItem : NetworkBehaviour
{
    private PlayerMove _playerController;

    public void Useitem(PlayerMove playerMove)
    {
        UseItemServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = true)]
    public void UseItemServerRpc(ulong clientId)
    {
        NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        PlayerMove playerController = playerObject.GetComponent<PlayerMove>();
        _playerController = playerController;

        switch (_playerController.my_ItemKind.Value)
        {
            case ItemKind.Gun:
                _playerController.UseGun();
                if (GameUI.Instance != null) GameUI.Instance.HideItem("Gun");
                break;

            case ItemKind.Speed:
                _playerController.CjangeSpeed();
                if (GameUI.Instance != null) GameUI.Instance.HideItem("Speed"); // 사용 후 UI 비활성화
                break;
        }
    }
}

