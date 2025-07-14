using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ItemSpawner : MonoBehaviour
{
    public GameObject coin;
    public GameObject[] Items;
    public Transform parentTransform;
    public int itemCount = 106;
    public float spawnRadius = 0.2f;
    public LayerMask obstacleMask;
    public NavMeshSurface navSurface;

    public void SpawnerOn()
    {

        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        List<Vector3> spawnPoints = new List<Vector3>();
        int itemAreaMask = 1 << NavMesh.GetAreaFromName("ItemSpawn");

        for (int i = 0; i < tri.indices.Length; i += 3)
        {
            Vector3 v0 = tri.vertices[tri.indices[i]];
            Vector3 v1 = tri.vertices[tri.indices[i + 1]];
            Vector3 v2 = tri.vertices[tri.indices[i + 2]];
            Vector3 center = (v0 + v1 + v2) / 3f;

            if (NavMesh.SamplePosition(center, out NavMeshHit hit, 0.5f, itemAreaMask))
            {
                if (!Physics.CheckSphere(hit.position, spawnRadius, obstacleMask))
                {
                    spawnPoints.Add(hit.position);
                }
            }
        }

        // 셔플
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            int randIndex = Random.Range(i, spawnPoints.Count);
            Vector3 temp = spawnPoints[i];
            spawnPoints[i] = spawnPoints[randIndex];
            spawnPoints[randIndex] = temp;
        }

        // 스폰 수 계산
        int totalSpawn = Mathf.Min(itemCount, spawnPoints.Count);
        int coinCount = Mathf.Min(100, totalSpawn);
        int otherCount = totalSpawn - coinCount;

        int spawnIndex = 0;

        Debug.Log("가능한 스폰 위치 수: " + spawnPoints.Count);

        // 1. 코인 먼저 스폰
        for (int i = 0; i < coinCount; i++)
        {
            Instantiate(coin, spawnPoints[spawnIndex], Quaternion.identity, parentTransform);
            spawnIndex++;
        }

        // 2. 나머지 아이템 랜덤 스폰
        for (int i = 0; i < otherCount; i++)
        {
            GameObject randomItem = Items[Random.Range(0, Items.Length)];
            Instantiate(randomItem, spawnPoints[spawnIndex], Quaternion.identity, parentTransform);
            spawnIndex++;
        }

    }

}
