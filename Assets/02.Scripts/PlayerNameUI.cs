using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNameUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText;
    private Transform cameraTransform;

    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        // 오너 플레이어라면 서버에 닉네임 등록
        if (IsOwner)
        {
            string nickname = AuthManager.Instance.CurrentNickname; // 본인 닉네임 가져오기
            SetPlayerNameServerRpc(nickname);

            // 자신의 카메라 찾아서 UI가 바라보게 하기
            Camera myCam = GetComponentInChildren<Camera>();
            if (myCam != null)
                cameraTransform = myCam.transform;
        }
        else
        {
            // 다른 플레이어는 로컬 오너의 카메라를 바라보게 하기
            cameraTransform = Camera.main != null ? Camera.main.transform : null;
        }

        // 닉네임 동기화 이벤트 연결
        playerName.OnValueChanged += OnNameChanged;
        nameText.text = playerName.Value.ToString();
    }

    private void OnDestroy()
    {
        playerName.OnValueChanged -= OnNameChanged;
    }

    private void Update()
    {
        if (cameraTransform != null)
        {
            transform.LookAt(cameraTransform);
            transform.Rotate(0, 180f, 0);
        }
    }

    private void OnNameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        nameText.text = newValue.ToString();
    }

    [ServerRpc]
    private void SetPlayerNameServerRpc(string newName)
    {
        playerName.Value = newName;
    }
}
