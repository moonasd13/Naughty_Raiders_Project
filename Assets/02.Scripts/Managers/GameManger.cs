using DefineEnum;
using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

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


    /// <summary>
    /// 상자 카운팅용 변수
    /// </summary>
    private int Room01_Score = 0;
    private int Room02_Score = 0;
    private int Room03_Score = 0;
    private int Room04_Score = 0;
    private bool countingEnd = false;

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

        switch (NowGameState.Value)
        {
            case GameState.Redy:
                if(is_GameStarted)
                {
                    GameStart();
                }
                //NowGameState.Value = GameState.firstTime;
                break;
            case GameState.firstTime:
                break;
            case GameState.secondTime: break;
        }
    }

    /// <summary>
    /// 점수 계산
    /// </summary>
    private void ScoreCounting()
    {
        int[] scores = new int[4];

        for (int i = 0; i < Room_Chests.Length; i++)
        {
            foreach (Transform child in Room_Chests[i].transform)
            {
                RoomItemBox box = child.GetComponent<RoomItemBox>();
                if (box != null)
                {
                    scores[i] += box.boxCount.Value;
                }
            }
        }

        Room01_Score = scores[0];
        Room02_Score = scores[1];
        Room03_Score = scores[2];
        Room04_Score = scores[3];

        countingEnd = true;

        if (countingEnd == true)
        {
            Debug.LogFormat($"{Room01_Score}, {Room02_Score}, {Room03_Score}, {Room04_Score}");
        }
    }

    /// <summary>
    /// 게임 시작시 세팅
    /// </summary>
    private void GameStart()
    {
        GameTimer.Instance.RequestStartTimerServerRpc();    // 이게 실행 되면 타이머가 돌아감
        Room01_Score = 0;
        Room02_Score = 0;
        Room03_Score = 0;
        Room04_Score = 0;
        
        itemSpawner.SpawnerOn();

        // 모든 플레이어 골드 초기화
        DatabaseManager.Instance.ResetAllPlayersGold();

        // 모든 플레이어 닉네임 + 골드 불러와서 UI 갱신
        DatabaseManager.Instance.LoadAllPlayersData();
    }


    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayersServerRpc()
    {
        ChangePositions();
    }

    private void ChangePositions()
    {
        foreach (NetworkPlayer player in NetworkPlayer.AllPlayers)
        {
            PlayerController playerCon = player.GetComponent<PlayerController>();
            ulong targetClientId = player.OwnerClientId;

            if (playerCon != null)
            {
                Vector3 targetPos = RoomsSPos[(int)targetClientId].position;
                Quaternion targetRot = RoomsSPos[(int)targetClientId].rotation;

                // 이동 로직 잠시 끄기
                playerCon.enabled = false;

                // CharacterController도 같이 끄기
                var controller = playerCon.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;

                // 좌표 강제 세팅
                playerCon.transform.SetPositionAndRotation(targetPos, targetRot);

                // 코루틴으로 한 프레임 뒤에 다시 켜기
                StartCoroutine(ReenableNextFrame(playerCon, controller, targetClientId, targetPos));
            }
        }
    }

    private IEnumerator ReenableNextFrame(PlayerController playerCon, CharacterController controller, ulong clientId, Vector3 targetPos)
    {
        yield return null; // 한 프레임 대기

        if (controller != null) controller.enabled = true;
        playerCon.enabled = true;

        Debug.Log($"플레이어 {clientId}를 {targetPos}로 이동 후 재활성화 완료");
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 120, 40), "Click Me"))
        {
            ScoreCounting();
        }

        if (GUI.Button(new Rect(10, 50, 120, 40), "Click Me"))
        {
            TeleportPlayersServerRpc();
        }
    }

    public void GameStateChange(GameState curState)
    {
        NowGameState.Value = curState;
    }
}
