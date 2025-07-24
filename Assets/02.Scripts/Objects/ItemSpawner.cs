using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class ItemSpawner : MonoBehaviour
{
    public GameObject coin;
    public GameObject[] Items;
    public Transform parentTransform;
    public GameObject SpawnPoses;
    public int CoinCount = 106;
    public int ItemCount = 4;

    List<Transform> SpawnList = new List<Transform>();

    public void Start()
    {
        SpawnList.Clear();

        foreach (Transform child in SpawnPoses.transform)
        {
            SpawnList.Add(child);
        }

        Debug.Log($"총 스폰 위치 수: {SpawnList.Count}");
    }

    public void SpawnerOn()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (SpawnList.Count < CoinCount)
        {
            Debug.LogWarning("스폰 위치보다 코인 개수가 많습니다. 일부는 생략됩니다.");
        }

        List<Transform> shuffledList = new List<Transform>(SpawnList);
        ShuffleList(shuffledList);

        int actualCoinCount = Mathf.Min(CoinCount, shuffledList.Count);
        int actualItemCount = Mathf.Min(ItemCount, shuffledList.Count - actualCoinCount);

        // 코인 생성
        for (int i = 0; i < actualCoinCount; i++)
        {
            Transform spawnPoint = shuffledList[i];
            GameObject coinObj = Instantiate(coin, spawnPoint.position, Quaternion.identity);
            coinObj.GetComponent<NetworkObject>().Spawn();
        }

        // 아이템 생성 (코인과 다른 위치에서)
        for (int i = 0; i < actualItemCount; i++)
        {
            Transform spawnPoint = shuffledList[actualCoinCount + i];
            GameObject randomItem = Items[Random.Range(0, Items.Length)];
            GameObject itemObj = Instantiate(randomItem, spawnPoint.position, Quaternion.identity);
            itemObj.GetComponent<NetworkObject>().Spawn();
        }
    }
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
