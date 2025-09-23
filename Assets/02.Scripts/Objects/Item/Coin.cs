using Unity.Netcode;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    private PlayerMove _playerController;

    public void GetCoin(PlayerMove player)
    {
        _playerController = player;
        
        if (_playerController.inHand.Value == false)
        {
            RequestPickupServerRpc();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong requestingClientId = rpcParams.Receive.SenderClientId;

        if (_playerController.OwnerClientId != requestingClientId)
            return;

        _playerController.inHand.Value = true;

        GetComponent<NetworkObject>().Despawn(true);
    }
}
