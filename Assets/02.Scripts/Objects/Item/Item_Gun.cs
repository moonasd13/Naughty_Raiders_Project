using Unity.Netcode;
using UnityEngine;

public class Item_Gun : ItemObject
{
    [SerializeField]
    public GameObject bullet;
    public Transform bulletPos;


    private Transform _targetHandTransform;

    public NetworkVariable<NetworkObjectReference> ParentNetObjectRef =
        new NetworkVariable<NetworkObjectReference>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Update()
    {
        // 장비 중이고, 타겟 Transform(_rHPos) 참조가 유효할 때만 실행
        if (equip.Value && _targetHandTransform != null)
        {
            // 1. 위치와 회전을 타겟 Transform의 월드 좌표와 일치시킵니다.
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
        _targetHandTransform = null;
    }


    private void UpdateParentAttachment(bool equipped)
    {
        if (equipped)
        {
            // 1. 장비 중일 때: 부모를 설정합니다.
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
            // 2. 장비 중이 아닐 때: 부모를 해제하고 월드에 배치합니다.
            // 부모를 null로 설정하여 부모-자식 관계를 해제하고, 월드 좌표를 유지합니다 (true).
            //transform.SetParent(null, true);
        }
    }

    private void SetAttachment(NetworkObject parentNetObj, Transform targetTransform)
    {
        // 1. [필수] 부모를 캐릭터 Root NetworkObject Transform으로 설정
        transform.SetParent(parentNetObj.transform, false);

        // 2. 타겟 Transform 참조 저장
        _targetHandTransform = targetTransform;

        // 3. (옵션) 최초 위치 설정 및 스케일 설정 (Update에서 계속 덮어쓸 것이므로, 사실상 Update만 돌려도 됨)
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    public override void OnNetworkDespawn()
    {
        ParentNetObjectRef.OnValueChanged -= OnParentChanged;
        equip.OnValueChanged -= OnEquippedChanged; // 이벤트 해제
        base.OnNetworkDespawn();
    }
}
