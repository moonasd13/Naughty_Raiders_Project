using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [SerializeField]
    private List<Transform> spawnPoints; // 인스펙터에서 1, 2, 3, 4 위치 넣기


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public Vector3 GetSpawnPosition(ulong clientId)
    {
        if (spawnPoints.Count == 0)
            return Vector3.zero;

        int index = (int)(clientId % (ulong)spawnPoints.Count);
        return spawnPoints[index].position;
    }

    public Quaternion GetSpawnRotation(ulong clientId)
    {
        if (spawnPoints.Count == 0)
            return Quaternion.identity;

        int index = (int)(clientId % (ulong)spawnPoints.Count);
        return spawnPoints[index].rotation;
    }
}
