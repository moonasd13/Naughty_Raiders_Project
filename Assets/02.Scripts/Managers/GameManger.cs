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

<<<<<<< Updated upstream

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
=======
    private DatabaseManager m_databaseManager;
    private int Room01_Score = 0;
    private int Room02_Score = 0;
    private int Room03_Score = 0;
    private int Room04_Score = 0;
    private bool countingEnd = false;
    private bool is_GameStart = false;
>>>>>>> Stashed changes

    private void Awake()
    {
        m_databaseManager = DatabaseManager.Instance;        
    }

    void Start()
    {
<<<<<<< Updated upstream
=======
       StartGame();
>>>>>>> Stashed changes
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
                    GameOverUI.Instance.ShowGameOver();
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
    }

    #region[캐릭터 텔레포트]

    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayersServerRpc()
    {
        ChangePositions();
    }

    public void StartGame()
    {
        //DatabaseManager에서 RoomId를 가져옴
        if (string.IsNullOrEmpty(m_databaseManager.CurrentRoomId))
        {
            Debug.LogWarning("RoomId가 설정되지 않았습니다!");
            return;
        }

        //모든 플레이어의 골드 초기화
        m_databaseManager.ResetAllPlayersGold();

        //플레이어 데이터 UI 갱신
        m_databaseManager.LoadAllPlayersData();
    }


    private void ChangePositions()
    {
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
}
