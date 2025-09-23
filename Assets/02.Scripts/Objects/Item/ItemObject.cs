using StarterAssets;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Define_Enums;

public class ItemObject : NetworkBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 45f;

    [Header("Floating Settings")]
    public float floatAmplitude = 0.25f;
    public float floatFrequency = 1f;

    private Vector3 _startPosition;

    ItemKind item_kind;
    Bullet codebullet;

    private PlayerMove _playerController;

    private void Start()
    {
        if (this.CompareTag("Item_Gun"))
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
    void Update()
    {
        if (!IsServer) return;

        Rotate();
        FloatUpAndDown();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _startPosition = transform.position;
        }
    }

    public void Getitem(PlayerMove player)
    {
        _playerController = player;

        if (_playerController.equip.Value == false)
        {
            RequestPickupServerRpc();
        }
    }

    /// <summary>
    /// 서버에게 실질적으로 수행해야하는 RPC전송
    /// </summary>
    /// <param name="requestingClientId"></param>
    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong requestingClientId = rpcParams.Receive.SenderClientId;

        if (_playerController.OwnerClientId != requestingClientId)
            return;

        _playerController.my_ItemKind.Value = item_kind;
        _playerController.equip.Value = true;

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
    /// 아이템 회전
    /// </summary>
    private void Rotate()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// 아이템 업다운
    /// </summary>
    private void FloatUpAndDown()
    {
        float newY = _startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
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
