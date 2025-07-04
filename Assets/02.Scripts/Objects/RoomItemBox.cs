using StarterAssets;
using UnityEngine;

public class RoomItemBox : MonoBehaviour
{
    [SerializeField]
    public BoxCollider RoomArea;
    public GameObject ScoreObject;
    [Header("Á¤º¸")]
    [SerializeField] public int boxCount { get; set; } = 0;

    private bool _isPlayerInZone = false;
    private Transform _playerHand;
    private PlayerController _playerController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInZone = true;
            PlayerController controller = other.GetComponent<PlayerController>();
            _playerHand = controller.lefthandTransform;
            _playerController = controller;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInZone = false;
            _isPlayerInZone = false;
            _playerHand = null;
            _playerController = null;
        }
    }

    private void Update()
    {
        if (_playerController == null || _playerHand == null)
            return;

        else if (_isPlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            if (_playerHand != null && _playerHand.childCount > 0)
            {
                Transform heldObject = _playerHand.GetChild(0);
                GameObject obj = heldObject.gameObject;

                _playerController.inHand = false;
                Destroy(heldObject.gameObject);
                boxCount++;
            }
            else if (_playerHand != null && _playerHand.childCount <= 0 && boxCount >= 1)
            {
                AttachToHand();
                boxCount--;
            }
        }
    }

    private void AttachToHand()
    {
        GameObject instance = Instantiate(ScoreObject, _playerHand);
        _playerController.inHand = true;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        instance.transform.GetChild(0).GetComponent<Collider>().enabled = false;
        instance.transform.GetChild(0).GetComponent<Coin>().enabled = false;
    }
}
