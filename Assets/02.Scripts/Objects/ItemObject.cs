using StarterAssets;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Define_Enums;

public class ItemObject : NetworkBehaviour
{
    [SerializeField]
    public Collider _senseZone;
    [SerializeField]
    public GameObject bullet;
    public Transform bulletPos;

    ItemKind item_kind;
    Bullet codebullet;
    private bool _isPlayerInZone = false;
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
                _isPlayerInZone = true;
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
                _isPlayerInZone = false;
                _firstPlayerController = null;
            }
        }
    }

    private void Update()
    {
        if (IsClient && _isPlayerInZone && _firstPlayerController != null && Input.GetKeyDown(KeyCode.E) && _firstPlayerController.equip.Value == false)
        {
            RequestPickupServerRpc(NetworkManager.LocalClientId);
        }
    }

    /// <summary>
    /// 탄환 RPC
    /// </summary>
    /// <param name="direction"></param>
    [ServerRpc]
    public void FireServerRpc(Vector3 direction)
    {
        Vector3 pos = bulletPos.transform.position;
        Quaternion fireRotation = Quaternion.LookRotation(direction);

        GameObject itemObj = Instantiate(bullet, pos, fireRotation);
        var netObj = itemObj.GetComponent<NetworkObject>();
        netObj.Spawn(true);

        Bullet codebullet = netObj.GetComponent<Bullet>();
        codebullet.SetDirection(direction);
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

        if (_firstPlayerController.equip.Value || !_isPlayerInZone)
            return;

        _firstPlayerController.equip.Value = true;
        _firstPlayerController.GetItemKind(item_kind);
        GetComponent<NetworkObject>().Despawn(true);
    }
}
