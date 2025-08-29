using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNameUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText;
    private Transform cameraTransform;

    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[PlayerNameUI] My Nickname at Spawn: {AuthManager.Instance.CurrentNickname}");
        // 닉네임 동기화 이벤트 연결
        playerName.OnValueChanged += OnNameChanged;
        nameText.text = playerName.Value.ToString();

        // 오너 플레이어라면 서버에 닉네임 등록
        if (IsOwner)
        {
            StartCoroutine(SetNameNextFrame());

        }
    }

    private IEnumerator SetNameNextFrame()
    {
        yield return null;
        string nickname = AuthManager.Instance.CurrentNickname; // 본인 닉네임 가져오기
        if (!string.IsNullOrEmpty(nickname))
        {
            SetPlayerNameServerRpc(nickname);
        }
    }

    public override void OnNetworkDespawn()
    {
        playerName.OnValueChanged -= OnNameChanged;
    }

    private void Update()
    {
        if (Camera.main != null)
        {
            nameText.transform.rotation = Quaternion.LookRotation(nameText.transform.position - Camera.main.transform.position);
        }
    }

    private void OnNameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        nameText.text = newValue.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string newName)
    {
        playerName.Value = newName;
    }
}
