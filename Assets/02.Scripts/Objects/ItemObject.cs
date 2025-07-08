using StarterAssets;
using System.Linq;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField]
    public GameObject _bullet;
    public Transform _bulletTransform;
    public Collider _senseZone;
    public float speed = 5;

    private bool _isPlayerInZone = false;
    private Transform _playerHand;
    private PlayerController _firstPlayerController;

    // 장비중, 사용전
    private bool _is_equip = false;
    private bool _is_Use = false;


    // 
    private Vector3 _dir;


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
    /// 부착
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
    /// 발사
    /// </summary>
    private void firing()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            _firstPlayerController.ShootiongaAnimation();
            _is_Use = true;
            Camera cam = Camera.main;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f ,0));
            RaycastHit hit;

            Vector3 TargetPoint;

            if(Physics.Raycast(ray, out hit, 100f))
                TargetPoint = hit.point;
            else
                TargetPoint = ray.GetPoint(100f);

            _dir = (TargetPoint - _bulletTransform.position).normalized;

        }
    }
    public void Shoot()
    {
        GameObject bulletObj =  Instantiate(_bullet, _bulletTransform.position, Quaternion.LookRotation(_dir));
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.Init(_dir, speed);
    }
}
