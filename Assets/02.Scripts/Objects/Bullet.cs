using StarterAssets;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    public Collider _senseZone;
    public float speed = 20;
    public float force = 500f;

    private PlayerController _firstPlayerController;
    private GameObject prison;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _firstPlayerController == null)
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                _firstPlayerController = controller;
                Transform playerParent = _firstPlayerController.transform.parent;
                if (playerParent != null)
                {
                    playerParent.position = prison.transform.position;
                }
                Destroy(gameObject);
            }
        }
    }

    void Start()
    {
        prison = GameObject.FindGameObjectWithTag("Prison");
        print(prison);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, 3f);
    }
}
