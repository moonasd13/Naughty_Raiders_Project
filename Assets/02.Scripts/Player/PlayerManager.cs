using StarterAssets;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : NetworkBehaviour
{
    [Header("플레이어 코드")]
    public ThirdPersonController ThirdPersonController;
    public PlayerInput PlayerInput;
    public StarterAssetsInputs Inputs;
    public AnimationSync AnimationSync;
    public GameObject CameraRoot;

    private CinemachineCamera _camera;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            ThirdPersonController.enabled = false;
            PlayerInput.enabled = false;
            Inputs.enabled = false;
        }
        else
        {
            _camera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<CinemachineCamera>();
            _camera.Follow = CameraRoot.transform;
        }
    }

    private void LateUpdate()
    {
        if (CameraRoot != null)
        {
            CameraRoot.transform.rotation = Quaternion.identity; // (0,0,0) 회전으로 고정
        }
    }
}
