using Define_Enums;
using StarterAssets;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerItem : NetworkBehaviour
{
    /// <summary>
    ///  속도 아이템 사용을 위한 저장값
    /// </summary>
    private float _defaultMoveSpeed;
    private float _defaultSprintSpeed;
    private float _speedIncrease = 1.3f;
    /// <summary>
    /// 스턴건에 필요한 변수
    /// </summary>
    [SerializeField] public Transform righthandTransform;
    public GameObject _gun;
    private Item_Gun _item_Gun;

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

        Debug.Log(_playerController);
        switch (_playerController.my_ItemKind.Value)
        {
            case ItemKind.Gun:
                GameObject itemObj = Instantiate(_gun, righthandTransform.position, righthandTransform.rotation);
                var netObj = itemObj.GetComponent<NetworkObject>();
                netObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                netObj.Spawn(true);
                netObj.TrySetParent(righthandTransform);

                _item_Gun = netObj.GetComponent<Item_Gun>();
                _item_Gun.equip = true;

                _playerController.in_action.Value = true;

                if (GameUI.Instance != null) GameUI.Instance.HideItem("Gun"); // 사용 후 UI 비활성화
                break;

            case ItemKind.Speed:
                _defaultMoveSpeed = _playerController.MoveSpeed.Value;
                _defaultSprintSpeed = _playerController.SprintSpeed.Value;
                _playerController.equip.Value = false;
                _playerController.my_ItemKind.Value = ItemKind.None;

                if (GameUI.Instance != null) GameUI.Instance.HideItem("Speed"); // 사용 후 UI 비활성화

                _playerController.MoveSpeed.Value *= _speedIncrease;
                _playerController.SprintSpeed.Value *= _speedIncrease;

                StartCoroutine(RevertSpeedAfterDelay(10f));
                break;
        }
    }

    // 속도 복구용 코루틴
    private IEnumerator RevertSpeedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        _playerController.MoveSpeed.Value = _defaultMoveSpeed;
        _playerController.SprintSpeed.Value = _defaultSprintSpeed;
    }

    // 슈팅 에니메이션 종료
    private void ShootingOff()
    {
        ShootoffServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = true)]
    public void ShootoffServerRpc(ulong clientId)
    {
        NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        PlayerMove playerController = playerObject.GetComponent<PlayerMove>();
        _playerController = playerController;

        _playerController.in_action.Value = false;
        _playerController.equip.Value = false;
        _playerController.my_ItemKind.Value = ItemKind.None;

        _item_Gun.gameObject.GetComponent<NetworkObject>().Despawn();
        _item_Gun = null;
    }
}
