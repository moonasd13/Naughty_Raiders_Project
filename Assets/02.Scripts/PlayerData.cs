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

    public NetworkVariable<int> CurrentItem = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public string FirebaseUid;

    private PlayerUI2 myUI;

    public static PlayerData LocalPlayer {  get; private set; }
    
    private void OnGoldChanged(int oldValue, int newValue)
    {
        GameUI.Instance.UpdatePlayer(this);
    }

    private void OnNickChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        GameUI.Instance.UpdatePlayer(this);
    }

    private void OnItemChanged(int oldValue, int newValue)
    {
        if (IsOwner && myUI != null)
        {
            myUI.UpdateItemUI(newValue);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalPlayer = this;

            myUI = GameUI.Instance.CreatePlayerUI(this);

            var playerMove = GetComponent<PlayerMove>();
            playerMove.SetUI(myUI);
        }

        CurrentItem.OnValueChanged += OnItemChanged;

        //스폰 직후 UI에 등록
        GameUI.Instance.RegisterPlayer(this);

        //값이 바뀌면 UI 갱신 요청
        Gold.OnValueChanged += OnGoldChanged;
        Nickname.OnValueChanged += OnNickChanged;

        //서버라면 Firebase에서 데이터 로드
        if (IsOwner)
        {
            string myUid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            SubmitFirebaseUidServerRpc(myUid);
        }
    }
    public override void OnNetworkDespawn()
    {
        Gold.OnValueChanged -= OnGoldChanged;
        Nickname.OnValueChanged -= OnNickChanged;
        //플레이어가 나가면 UI에서 제거
        GameUI.Instance.UnregisterPlayer(this);
        CurrentItem.OnValueChanged -= OnItemChanged;
    }

    [ServerRpc]
    private void SubmitFirebaseUidServerRpc(string uid)
    {
        FirebaseUid = uid;

        // 서버에서 DB 데이터 불러오기
        DatabaseManager.Instance.LoadGoldFromDatabase(this, FirebaseUid);
        DatabaseManager.Instance.LoadNickFromDatabase(this, FirebaseUid);
    }

    [ServerRpc]
    public void ChangeGoldServerRpc(int amount)
    {
        Gold.Value += amount;

        // Firebase에도 반영
        if (!string.IsNullOrEmpty(FirebaseUid))
        {
            DatabaseManager.Instance.SaveGoldToDatabase(FirebaseUid, Gold.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetGoldFromDatabaseServerRpc(int gold)
    {
        Gold.Value = gold;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetNicknameFromDatabaseServerRpc(string nickname)
    {
        Nickname.Value = nickname;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetGoldServerRpc()
    {
        Gold.Value = 0;
    }
}
