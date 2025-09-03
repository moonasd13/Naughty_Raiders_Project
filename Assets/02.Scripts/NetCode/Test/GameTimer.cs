using Unity.Netcode;
using UnityEngine;
using System;

public class GameTimer : NetworkBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("설정")]
    [Tooltip("매치 총 시간(초). 10분 = 600")]
    [SerializeField] private float matchDurationSeconds = 600f;

    [Header("동기화 변수")]
    // 남은 시간(초). 서버만 쓰기 가능, 모두 읽기 가능
    public NetworkVariable<float> TimeLeft = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsRunning = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public event Action OnTimerEnded; // 로컬 이벤트(클라/서버 공통)

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

    public override void OnNetworkSpawn()
    {
        // 네트워크에 올라오면 초기화(서버에서만)
        if (IsServer)
        {
            TimeLeft.Value = matchDurationSeconds;
            IsRunning.Value = false;
        }
    }

    private void Update()
    {
        // 서버에서만 감소시킴 -> 값은 자동으로 클라에 동기화
        if (!IsServer || !IsRunning.Value) return;

        float newTime = TimeLeft.Value - Time.deltaTime;
        TimeLeft.Value = Mathf.Max(0f, newTime);

        if (TimeLeft.Value <= 0f)
        {
            IsRunning.Value = false;
            // 끝났음을 클라에도 알림(옵션)
            TimerEndedClientRpc();
            OnTimerEnded?.Invoke();
        }
    }

    /// <summary>서버/호스트에서 매치 시작</summary>
    public void StartTimerServer()
    {
        if (!IsServer) return;
        TimeLeft.Value = matchDurationSeconds;
        IsRunning.Value = true;
    }

    /// <summary>클라이언트가 서버에 타이머 시작 요청(권한 없이도 가능)</summary>
    [ServerRpc(RequireOwnership = false)]
    public void RequestStartTimerServerRpc()
    {
        StartTimerServer();
    }

    /// <summary>모든 클라에게 타이머 종료 신호(필요 시 게임오버 UI 등에서 사용)</summary>
    [ClientRpc]
    private void TimerEndedClientRpc()
    {
        OnTimerEnded?.Invoke();
    }

    /// <summary>외부에서 시간 재설정(서버만)</summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetTimeServerRpc(float seconds)
    {
        if (!IsServer) return;
        TimeLeft.Value = Mathf.Max(0f, seconds);
    }

    /// <summary>타이머 일시정지/재개(서버만)</summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetRunningServerRpc(bool running)
    {
        if (!IsServer) return;
        IsRunning.Value = running;
    }
}