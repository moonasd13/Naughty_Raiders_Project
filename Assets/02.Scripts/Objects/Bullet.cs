using StarterAssets;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    public Collider _senseZone;
    private PlayerController _firstPlayerController;


    public void Init(Vector3 direction, float speed)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = direction * speed;

        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _firstPlayerController == null)
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                _firstPlayerController = controller;
                Debug.LogFormat("플레이어 명중");
                Destroy(gameObject);
            }
        }

        Destroy(gameObject);
    }
}
