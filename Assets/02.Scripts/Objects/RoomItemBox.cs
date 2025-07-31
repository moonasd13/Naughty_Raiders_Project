using StarterAssets;
using Unity.Netcode;
using UnityEngine;

public class RoomItemBox : NetworkBehaviour
{
    [SerializeField]
    public BoxCollider RoomArea;
    public GameObject ScoreObject;
    [Header("정보")]
    public NetworkVariable<int> boxCount = new NetworkVariable<int>(0);
    //[SerializeField] public int boxCount { get; set; } = 0;

    private bool _isPlayerInZone = false;
    private PlayerController _playerController;

    [SerializeField] private int rewardPerBox = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {            
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                _isPlayerInZone = true;
                _playerController = controller;
            }            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller == _playerController)
            {
                _isPlayerInZone = false;
                _playerController = null;
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (_isPlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            SubmitInteractServerRpc(NetworkManager.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitInteractServerRpc(ulong clientId)
    {
        if (_playerController == null || _playerController.OwnerClientId != clientId)
            return;

        if (_playerController.inHand)
        {
            _playerController.inHand = false;
            boxCount.Value++;

            // 골드 증가 처리
            int reward = rewardPerBox;
            SendGoldToClientRpc(clientId, reward);
        }
        else if (!_playerController.inHand && boxCount.Value > 0)
        {
            _playerController.inHand = true;
            boxCount.Value--;
        }
    }

    [ClientRpc]
    private void SendGoldToClientRpc(ulong clientId, int goldAmount)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            DatabaseManager.Instance.ChangeGold(goldAmount);
        }
    }
}
