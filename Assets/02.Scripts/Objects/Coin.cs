using StarterAssets;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField]
    public Collider _senseZone;

    private bool _isPlayerInZone = false;
    private Transform _playerHand;
    private PlayerController _firstPlayerController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _firstPlayerController == null)
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                _isPlayerInZone = true;
                _firstPlayerController = controller;
                _playerHand = controller.lefthandTransform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller == _firstPlayerController)
            {
                _isPlayerInZone = false;
                _playerHand = null;
                _firstPlayerController = null;
            }
        }
    }

    private void Update()
    {
        if (_isPlayerInZone && _firstPlayerController != null && Input.GetKeyDown(KeyCode.E) && _firstPlayerController.inHand == false)
        {
            AttachToHand();
        }
    }

    private void AttachToHand()
    {
        _firstPlayerController.inHand = true;
        transform.SetParent(_playerHand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        _isPlayerInZone = false;
        _playerHand = null;
        _firstPlayerController = null;
        GetComponent<Collider>().enabled = false;
        this.enabled = false;
    }
}
