using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ItemSpawner : MonoBehaviour
{
    public GameObject prefab;
    public int itemCount = 20;
    public float spawnRadius = 0.5f;
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

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            int randIndex = Random.Range(i, spawnPoints.Count);
            Vector3 temp = spawnPoints[i];
            spawnPoints[i] = spawnPoints[randIndex];
            spawnPoints[randIndex] = temp;
        }

        for (int i = 0; i < Mathf.Min(itemCount, spawnPoints.Count); i++)
        {
            Instantiate(prefab, spawnPoints[i], Quaternion.identity);
        }
    }
}
