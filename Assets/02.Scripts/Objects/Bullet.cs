using StarterAssets;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField]
    public Collider _senseZone;
    private PlayerController _firstPlayerController;

    private Vector3 moveDirection;
    public float rayDistance = 0.5f;
    public float moveSpeed;

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    private void Update()
    {
        if (!IsServer) return;

        Ray ray = new Ray(transform.position, moveDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                // º®¿¡ ºÎµúÈû
                Debug.Log("º® Ãæµ¹ ¡æ ÆÄ±«µÊ");
                GetComponent<NetworkObject>().Despawn(true);
                return;
            }
        }
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        Debug.Log("ÅºÈ¯");
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
                Debug.Log(other + "¸íÁß");
            }
        }

        GetComponent<NetworkObject>().Despawn(true);
    }
}
