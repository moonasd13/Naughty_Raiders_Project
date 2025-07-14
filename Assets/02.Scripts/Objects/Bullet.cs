using StarterAssets;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    public Collider _senseZone;
    private PlayerController _firstPlayerController;

    private Vector3 moveDirection;
    public float moveSpeed;

    public void Init(Vector3 direction, float speed)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
    }

    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        Debug.DrawRay(transform.position, moveDirection * 3f, Color.red, 2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _firstPlayerController == null)
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                _firstPlayerController = controller;
                _firstPlayerController.stunON();
                Debug.LogFormat("플레이어 명중: {0}", other.transform.parent.name);
                //Destroy(gameObject);
            }
        }

        //Destroy(gameObject);
    }
}
