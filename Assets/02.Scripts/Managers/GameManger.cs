using DefineEnum;
using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class GameManger : NetworkBehaviour
{
    public static GameManger Instance { get; private set; }

    [Header("컴포넌트")]
    [SerializeField]
    public GateController gateController;
    public ItemSpawner itemSpawner;
    [SerializeField]
    [Header("플레이어 시작지점")]
    public GameObject[] Room_Chests;
    public Transform[] RoomsSPos;


    private bool is_GameStarted = false;
    private bool is_playersPosChanged = false;
    private bool is_GateOpen = false;
    private float currentTime;

    private NetworkVariable<GameState> NowGameState = new NetworkVariable<GameState>(
     GameState.Redy,
     NetworkVariableReadPermission.Everyone,
     NetworkVariableWritePermission.Server
 );
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
    }

    void Update()
    {
        if (!IsServer) return;

        currentTime = GameTimer.Instance.TimeLeft.Value;

        switch (NowGameState.Value)
        {
            case GameState.Redy:
                if(!is_GameStarted)
                {
                    GameStart();
                    is_GameStarted = true;
                }
                break;
            case GameState.firstTime:
                if(!is_playersPosChanged)
                {
                    TeleportPlayersServerRpc();
                    GameTimer.Instance.RequestStartTimerServerRpc();    // 이게 실행 되면 타이머가 돌아감
                    is_playersPosChanged = true;
                }

                if (currentTime <= 590) //560
                {
                    if(!is_GateOpen)
                    {
                        gateController.OpenGate();
                        is_GateOpen = true;
                    }
                }

                break;
            case GameState.secondTime:
                if (currentTime <= 0)
                {
                    Debug.Log("game end"); 
                }
                    break;
        }
    }

    /// <summary>
    /// 게임 시작시 세팅
    /// </summary>
    private void GameStart()
    {
        itemSpawner.SpawnerOn();

        // 모든 플레이어 골드 초기화
        DatabaseManager.Instance.ResetAllPlayersGold();

        //// 모든 플레이어 닉네임 + 골드 불러와서 UI 갱신
        //DatabaseManager.Instance.LoadAllPlayersData();
    }

    #region[캐릭터 텔레포트]

    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayersServerRpc()
    {
        ChangePositions();
    }

    // GameManger.cs (수정된 ChangePositions 메서드)

    private void ChangePositions()
    {
        DebugPrintAllPlayers();

        foreach (NetworkPlayer player in NetworkPlayer.AllPlayers)
        {
            PlayerMove playerCon = player.GetComponent<PlayerMove>();
            ulong targetClientId = player.OwnerClientId;

            if (playerCon != null)
            {
                Vector3 targetPos = RoomsSPos[(int)targetClientId].position;
                Quaternion targetRot = RoomsSPos[(int)targetClientId].rotation;

                ClientRpcSendParams rpcSendParams = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { targetClientId }
                };

                playerCon.TeleportTargetClientRpc(targetPos, targetRot);
            }
        }
    }
    #endregion


    public void GameStateChange(GameState curState)
    {
        NowGameState.Value = curState;
    }

    public static void DebugPrintAllPlayers()
    {
        // 리스트가 비어있는지 확인
        if (NetworkPlayer.AllPlayers.Count == 0)
        {
            Debug.Log("DEBUG: AllPlayers 리스트가 비어 있습니다.");
            return;
        }

        // 리스트의 총 개수를 먼저 출력
        Debug.Log($"DEBUG: === AllPlayers 리스트 ({NetworkPlayer.AllPlayers.Count}명) 출력 시작 ===");

        // 리스트의 각 항목을 순회하며 정보 출력
        for (int i = 0; i < NetworkPlayer.AllPlayers.Count; i++)
        {
            NetworkPlayer player = NetworkPlayer.AllPlayers[i];

            // 플레이어 객체가 null이 아닌지 확인
            if (player != null)
            {
                // 각 플레이어의 중요한 정보를 출력
                Debug.Log($"[{i}] Player Name: {player.gameObject.name} | " +
                          $"Client ID: {player.OwnerClientId} | " +
                          $"Network ID: {player.NetworkObjectId}");
            }
            else
            {
                Debug.Log($"[{i}] Player: NULL (리스트에서 객체를 찾을 수 없음)");
            }
        }

        Debug.Log("DEBUG: === AllPlayers 리스트 출력 종료 ===");
    }
}
