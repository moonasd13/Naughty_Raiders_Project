using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    private PlayerMove _playerController;

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
        _playerController = player;

        if (_playerController.inHand.Value == false)
        {
            RequestPickupServerRpc();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong requestingClientId = rpcParams.Receive.SenderClientId;

        NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[requestingClientId].PlayerObject;
        PlayerMove playerController = playerObject.GetComponent<PlayerMove>();

        if (playerController == null)
        {
            Debug.LogError("PlayerController 못 찾음!");
            return;
        }

        _playerController = playerController; // 안전하게 저장

        _playerController.inHand.Value = true;

        Destroy(this.gameObject);
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
