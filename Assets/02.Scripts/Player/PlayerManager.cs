using StarterAssets;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : NetworkBehaviour
{
    [Header("플레이어 코드")]
    public PlayerMove PlayerMove;
    public GameObject CameraRoot;

    private CinemachineCamera _camera;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            PlayerMove.enabled = false;
        }
        else
        {
            _camera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<CinemachineCamera>();
            _camera.Follow = CameraRoot.transform;
        }
    }
}
