using StarterAssets;
using Unity.Netcode;
using UnityEngine;


public class AnimationSync : NetworkBehaviour
{
    public ThirdPersonController _thirdPersonController;
    public Animator _animator;

    private int _animIDSpeed;
    private int _animIDGrounded;

    void Awake()
    {

        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
    }

    void Update()
    {
        if (_thirdPersonController == null || _animator == null) return;

        // 1. Public 변수 직접 읽기
        bool isGrounded = _thirdPersonController.Grounded;

        // 2. Private 변수 대신 Animator 파라미터 값 읽기
        float currentSpeed = _animator.GetFloat(_animIDSpeed);
        bool isAnimatorGrounded = _animator.GetBool(_animIDGrounded);
    }
}
