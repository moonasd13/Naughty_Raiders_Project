using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<int> Gold = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<FixedString32Bytes> Nickname = new NetworkVariable<FixedString32Bytes>(
        "NoName",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // 스폰 직후 UI에 등록
        GameUI.Instance.RegisterPlayer(this);

        // 값이 바뀌면 UI 갱신 요청
        Gold.OnValueChanged += (oldValue, newValue) =>
        {
            GameUI.Instance.UpdatePlayer(this);
        };

        Nickname.OnValueChanged += (oldValue, newValue) =>
        {
            GameUI.Instance.UpdatePlayer(this);
        };

        // 서버라면 Firebase에서 데이터 로드
        if (IsServer)
        {
            DatabaseManager.Instance.LoadGoldFromDatabase(this);
            DatabaseManager.Instance.LoadNickFromDatabase(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        // 플레이어가 나가면 UI에서 제거
        GameUI.Instance.UnregisterPlayer(this);
    }
}
