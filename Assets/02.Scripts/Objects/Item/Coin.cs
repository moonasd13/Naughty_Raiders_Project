using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 45f;

    [Header("Floating Settings")]
    public float floatAmplitude = 0.25f;
    public float floatFrequency = 1f;

    private Vector3 _startPosition;

    private void Update()
    {
        Rotate();
        FloatUpAndDown();
    }

    public void GetCoin(PlayerMove player)
    {
        if (player.inHand.Value == false)
        {
            RequestPickupServerRpc(player.NetworkObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ulong playerNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var playerObj))
        {
            var playerMove = playerObj.GetComponent<PlayerMove>();
            if (playerMove != null && !playerMove.inHand.Value)
            {
                playerMove.inHand.Value = true;
            }
        }

        // 코인 삭제
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null) netObj.Despawn();
        else Destroy(gameObject);
    }

    /// <summary>
    /// 아이템 회전
    /// </summary>
    private void Rotate()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// 아이템 업다운
    /// </summary>
    private void FloatUpAndDown()
    {
        float newY = _startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
