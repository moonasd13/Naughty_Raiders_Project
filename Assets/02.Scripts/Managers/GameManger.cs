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
    private bool is_GameOverTriggered = false;
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

    void Update()
    {
        if (!IsServer) 
        {
            return;
        }

        if (GameTimer.Instance == null)
        {
            return;
        }

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
                    GameTimer.Instance.RequestStartTimerServerRpc();    //이게 실행 되면 타이머가 돌아감
                    is_playersPosChanged = true;
                }

                if (currentTime <= 560) //560
                {
                    if(!is_GateOpen)
                    {
                        gateController.OpenGate();
                        is_GateOpen = true;
                    }
                }

                if (currentTime <= 0)
                {
                    if (!is_GameOverTriggered)
                    {
                        is_GameOverTriggered = true;
                        ShowGameOverClientRpc();
                    }
                }
                break;

            case GameState.secondTime:

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

    [ClientRpc]
    private void ShowGameOverClientRpc()
    {
        Debug.Log($"[CLIENT RPC] Client {NetworkManager.Singleton.LocalClientId} -> GameOver 호출");
        GameOverUI.Instance?.ShowLocalGameOver();
    }
}
