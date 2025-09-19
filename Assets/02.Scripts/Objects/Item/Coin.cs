using StarterAssets;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    [SerializeField]
    public Collider _senseZone;

    //private bool _isPlayerInZone = false;
    private PlayerController _firstPlayerController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _firstPlayerController == null)
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                //_isPlayerInZone = true;
                _firstPlayerController = controller;

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller == _firstPlayerController)
            {
                //_isPlayerInZone = false;
                _firstPlayerController = null;
            }
        }
    }

    private void Update()
    {
        // 물건을 잡을수 있는 조건
        //if (IsClient && _isPlayerInZone && _firstPlayerController != null && Input.GetKeyDown(KeyCode.E) && _firstPlayerController.inHand.Value == false)
        //{
        //    RequestPickupServerRpc(NetworkManager.LocalClientId);
        //}
    }

    /// <summary>
    /// 서버에게 실질적으로 수행해야하는 RPC전송
    /// </summary>
    /// <param name="requestingClientId"></param>
    //[ServerRpc(RequireOwnership = false)]
    //private void RequestPickupServerRpc(ulong requestingClientId)
    //{
    //    if (_firstPlayerController == null || _firstPlayerController.OwnerClientId != requestingClientId)
    //        return;

    //    //if (_firstPlayerController.inHand.Value || !_isPlayerInZone)
    //    //    return;

    //    //_firstPlayerController.inHand.Value = true;

    //    GetComponent<NetworkObject>().Despawn(true);
    //}
}
