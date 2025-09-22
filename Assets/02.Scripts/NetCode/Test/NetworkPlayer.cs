using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField]
    private Mesh[] PlayerMesh;
    private SkinnedMeshRenderer SkinnedMeshRenderer;

    // 어디서든 접근가능한 플레이어 리스틀 만들고 이곳에 저장
    public static List<NetworkPlayer> AllPlayers { get; private set; } = new List<NetworkPlayer>();
    public override void OnNetworkSpawn()
    {
        //생성시 리스트에 자기자신을 추가
        AllPlayers.Add(this);
    }
    public override void OnNetworkDespawn()
    {
        // 리스트에서 자기 자신을 제거
        AllPlayers.Remove(this);
    }

    private void Start()
    {
        SkinnedMeshRenderer = gameObject.transform.GetComponentInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer.sharedMesh = PlayerMesh[OwnerClientId];

    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveRequestServerRpc(Vector3 newPosition, Quaternion newRotation)
    {
        transform.SetPositionAndRotation(newPosition, newRotation);
    }
}
