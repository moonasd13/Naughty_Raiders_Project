using UnityEngine;
using Unity.Netcode;

public class FloatingItem : NetworkBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 45f;

    [Header("Floating Settings")]
    public float floatAmplitude = 0.25f;
    public float floatFrequency = 1f;

    private Vector3 _startPosition;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _startPosition = transform.position;
        }
    }

    void Update()
    {
        if (!IsServer) return; // 서버만 움직임을 계산함

        Rotate();
        FloatUpAndDown();
    }

    private void Rotate()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void FloatUpAndDown()
    {
        float newY = _startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
