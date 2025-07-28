using StarterAssets;
using Unity.Netcode;
using UnityEngine;

public class RoomItemBox : NetworkBehaviour
{
    [SerializeField]
    public BoxCollider RoomArea;
    public GameObject ScoreObject;
    [Header("Á¤º¸")]
    [SerializeField] public int boxCount { get; set; } = 0;

    private bool _isPlayerInZone = false;
    private PlayerController _playerController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInZone = true;
            PlayerController controller = other.GetComponent<PlayerController>();
            _playerController = controller;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInZone = false;
            _playerController = null;
        }
    }

    private void Update()
    {
        if (_playerController == null)
            return;

        if (IsClient && _isPlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            if (_playerController.inHand)
            {
                _playerController.inHand = false;
                boxCount++;
            }
            else if (!_playerController.inHand && boxCount >= 1)
            {
                _playerController.inHand = true;
                boxCount--;
            }
        }
    }
}
