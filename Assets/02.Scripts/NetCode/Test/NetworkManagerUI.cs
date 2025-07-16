using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] Button serverBtn;
    [SerializeField] Button hostBtn;
    [SerializeField] Button clientBtn;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject[] spawnPlayerPrefabs;

    void Awake()
    {

        serverBtn.AddEvent(() =>
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.StartServer();
            Debug.Log("서버로 시작했습니다.");
        });

        hostBtn.AddEvent(() =>
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.StartHost();
            Debug.Log("호스트로 시작했습니다.");
        });

        clientBtn.AddEvent(() =>
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("클라이언트로 접속을 시도했습니다.");
        });
    }
    private void HandleClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // 클라이언트 ID를 기반으로 인덱스 선택 (최대 길이 초과 방지)
        int index = (int)(clientId % (ulong)spawnPlayerPrefabs.Length);
        GameObject playerPrefab = spawnPlayerPrefabs[index];
        if (playerPrefab == null)
        {
            Debug.LogError($"Player 프리팹이 비어 있습니다. index: {index}");
            return;
        }

        // 해당 index에 맞는 스폰 위치 사용
        Vector3 pos = spawnPoints[index % spawnPoints.Length].position;
        Quaternion rot = spawnPoints[index % spawnPoints.Length].rotation;

        GameObject obj = Instantiate(playerPrefab, pos, rot);
        obj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        Debug.Log($"플레이어 수동 생성 완료: ClientId {clientId}, 프리팹 이름: {playerPrefab.name}");
    }
}

public static class Extension
{
    public static void AddEvent(this Button btn, UnityAction action)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }
}
