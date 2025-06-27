using StarterAssets;
using UnityEngine;

public class RoomItem : MonoBehaviour
{
    [SerializeField]
    public BoxCollider RoomArea;
    public Transform ScoreObj;
    [SerializeField]
    public Vector3 offset = new Vector3(1.5f, 0, 0);

    private bool _isPlayerInZone = false;
    private Transform _playerHand;
    private PlayerController _playerController;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInZone = true;
            PlayerController controller = other.GetComponent<PlayerController>();
            _playerHand = controller.handTransform;
            _playerController = controller;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInZone = false;
        }
    }

    private void Update()
    {
        if (_playerController == null || _playerHand == null)
            return;

        if (_isPlayerInZone && Input.GetKeyDown(KeyCode.F))
        {
            if(_playerHand != null && _playerHand.childCount > 0)
            {
                Transform heldObject = _playerHand.GetChild(0);
                GameObject obj = heldObject.gameObject;

                _playerController.inHand = false;
                heldObject.SetParent(ScoreObj);
                heldObject.gameObject.SetActive(false);
                ItemBox itemBox = obj.GetComponent<ItemBox>();
                itemBox.enabled = true;

                SpawnNextObject(obj);
            }
        }
    }


    void SpawnNextObject(GameObject obj)
    {
        Vector3 basePosition = ScoreObj.position;

        int currentCount = ScoreObj.childCount;
        Vector3 spawnPosition = basePosition + offset * currentCount;
        obj.transform.position = spawnPosition;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.GetComponent<Collider>().enabled = true;
        obj.SetActive(true);
    }
}
