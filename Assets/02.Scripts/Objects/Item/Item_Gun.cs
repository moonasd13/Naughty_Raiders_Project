using Unity.Netcode;
using UnityEngine;

public class Item_Gun : ItemObject
{
    [SerializeField]
    public GameObject bullet;
    public Transform bulletPos;
    private Transform _targetHandTransform;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 두 변수의 변경 이벤트를 모두 구독합니다.
        ParentNetObjectRef.OnValueChanged += OnParentChanged;
        equip.OnValueChanged += OnEquippedChanged;

        // 스폰 시 초기 상태를 반영합니다.
        UpdateParentAttachment(equip.Value);
    }

    // ParentNetObjectRef가 변경될 때 호출됩니다.
    private void OnParentChanged(NetworkObjectReference oldValue, NetworkObjectReference newValue)
    {
        // 부모 객체 참조가 변경되었을 때, 현재 장비 상태를 기반으로 처리합니다.
        UpdateParentAttachment(equip.Value);
    }

    // IsEquipped 상태가 변경될 때 호출됩니다.
    private void OnEquippedChanged(bool oldValue, bool newValue)
    {
        // 장비 상태가 변경되었을 때, 새 상태를 기반으로 처리합니다.
        UpdateParentAttachment(newValue);
    }


    private void UpdateParentAttachment(bool equipped)
    {
        if (equipped)
        {
            if (ParentNetObjectRef.Value.TryGet(out NetworkObject parentNetObj))
            {
                // PlayerMove는 NetworkObject와 같은 GameObject에 있다고 가정합니다.
                PlayerMove playerMove = parentNetObj.gameObject.GetComponent<PlayerMove>();

                if (playerMove != null && playerMove._rHPos != null)
                {
                    // SetAttachment에 NetworkObject와 타겟 Transform 모두 전달
                    SetAttachment(parentNetObj, playerMove._rHPos);
                }
            }
        }
        else
        {
            _targetHandTransform = null;
        }
    }

    void LateUpdate()
    {
        if (equip.Value && _targetHandTransform != null)
        {
            transform.position = _targetHandTransform.position;
            transform.rotation = _targetHandTransform.rotation;
        }
    }


    [ServerRpc]
    public override void UseServerRpc(Vector3 direction)
    {
        Vector3 pos = bulletPos.transform.position;
        Quaternion fireRotation = Quaternion.LookRotation(direction);

        GameObject itemObj = Instantiate(bullet, pos, fireRotation);
        var netObj = itemObj.GetComponent<NetworkObject>();
        netObj.Spawn(true);

        Bullet codebullet = netObj.GetComponent<Bullet>();
        codebullet.SetDirection(direction);
    }

    private void SetAttachment(NetworkObject parentNetObj, Transform targetTransform)
    {
        transform.SetParent(parentNetObj.transform, false);

        _targetHandTransform = targetTransform;

        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }
}
