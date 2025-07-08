using StarterAssets;
using System.Linq;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField]
    public GameObject _bullet;
    public Transform _bulletTransform;
    public Collider _senseZone;

    private bool _isPlayerInZone = false;
    private Transform _playerHand;
    private PlayerController _firstPlayerController;
    private bool _is_equip = false;
    private bool _is_Use = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _firstPlayerController == null)
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                _isPlayerInZone = true;
                _firstPlayerController = controller;
                _playerHand = controller.righthandTransform;
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
        if (_isPlayerInZone && _firstPlayerController != null && Input.GetKeyDown(KeyCode.E) && _firstPlayerController.equip == false)
        {
            AttachToHand();
        }

        if (_is_Use == false && _is_equip)
        {
            firing();
        }
    }

    /// <summary>
    /// ºÎÂø
    /// </summary>
    private void AttachToHand()
    {
        _firstPlayerController.equip = true;
        transform.SetParent(_playerHand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        GetComponent<Collider>().enabled = false;
        _is_equip = true;
    }

    /// <summary>
    /// ¹ß»ç
    /// </summary>
    private void firing()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            _firstPlayerController.ShootiongaAnimation();
            _is_Use = true;
        }
    }
    public void Shoot()
    {
        Instantiate(_bullet, _bulletTransform.position, _bulletTransform.rotation);
    }
}
