using StarterAssets;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Define_Enums;

public class ItemObject : NetworkBehaviour
{
    [SerializeField]
    public Collider _senseZone;
    
    ItemKind item_kind;
    Bullet codebullet;
    //private bool _isPlayerInZone = false;
    private PlayerController _firstPlayerController;

    private void Start()
    {
        if(this.CompareTag("Item_Gun"))
        {
            item_kind = ItemKind.Gun;
        }
        else if (this.CompareTag("Item_Speed"))
        {
            item_kind = ItemKind.Speed;
        }
        else
        {
            Debug.Log("아이템 태그 없음");
        }

    }

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
                //_firstPlayerController = null;
            }
        }
    }

    private void Update()
    {
        //if (IsClient && _isPlayerInZone && _firstPlayerController != null && Input.GetKeyDown(KeyCode.E) && _firstPlayerController.equip.Value == false)
        //{
        //    RequestPickupServerRpc(NetworkManager.LocalClientId);
        //}
    }

    /// <summary>
    /// 서버에게 실질적으로 수행해야하는 RPC전송
    /// </summary>
    /// <param name="requestingClientId"></param>
    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ulong requestingClientId)
    {
        if (_firstPlayerController == null || _firstPlayerController.OwnerClientId != requestingClientId)
            return;

        //if (_firstPlayerController.equip.Value || !_isPlayerInZone)
        //    return;

        //_firstPlayerController.equip.Value = true;
        //_firstPlayerController.GetItemKind(item_kind);

        if (this.CompareTag("Item_Gun"))
        {
            if (IsOwner && GameUI.Instance != null) GameUI.Instance.ShowItem("Gun");
        }
        else if (this.CompareTag("Item_Speed"))
        {
            if (IsOwner && GameUI.Instance != null) GameUI.Instance.ShowItem("Speed"); 
        }

            GetComponent<NetworkObject>().Despawn(true);

    }



    /// <summary>
    /// 자식에게 상속
    /// </summary>
    /// <param name="direction"></param>
    [ServerRpc]
    public virtual void UseServerRpc(Vector3 direction)
    {
        Debug.Log("부모 아이템은 기본 사용 기능 없음");
    }


}
