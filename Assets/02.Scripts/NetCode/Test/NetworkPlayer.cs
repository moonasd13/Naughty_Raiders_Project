using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsServer) // 서버(호스트)에서만 위치를 설정해야 모든 클라이언트에 동기화됨
        {
            Vector3 spawnPos = SpawnManager.Instance.GetSpawnPosition(OwnerClientId);
            Quaternion spawnRot = SpawnManager.Instance.GetSpawnRotation(OwnerClientId);

            transform.SetPositionAndRotation(spawnPos, spawnRot);
        }
    }
}
