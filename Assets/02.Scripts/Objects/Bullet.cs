using StarterAssets;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField]
    public Collider _senseZone;
    private PlayerController _firstPlayerController;

    private Vector3 moveDirection;
    public float moveSpeed;

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    private void Update()
    {
        if (!IsServer) return;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        Debug.Log("≈∫»Ø");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player") && _firstPlayerController == null)
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                _firstPlayerController = controller;
                _firstPlayerController.stunON();
                Debug.Log(other + "∏Ì¡ﬂ");
            }
        }

        GetComponent<NetworkObject>().Despawn(true);
    }
}
