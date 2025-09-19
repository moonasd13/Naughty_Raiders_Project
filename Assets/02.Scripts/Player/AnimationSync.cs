using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class AnimationSync : NetworkBehaviour
{
    public Animator _animator;

    private NetworkVariable<float> netSpeed = new NetworkVariable<float>();
    private NetworkVariable<bool> netGrounded = new NetworkVariable<bool>();
    private NetworkVariable<bool> netJump = new NetworkVariable<bool>();
    private NetworkVariable<bool> netFreeFall = new NetworkVariable<bool>();
    private NetworkVariable<float> netMotion = new NetworkVariable<float>();

    private int _animIDSpeed = Animator.StringToHash("Speed");
    private int _animIDGrounded = Animator.StringToHash("Grounded");
    private int _animIDJump = Animator.StringToHash("Jump");
    private int _animIDFreeFall = Animator.StringToHash("FreeFall");
    private int _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

    private void Start()
    {
        // 네트워크 변수 값이 바뀌면 Animator 업데이트
        netSpeed.OnValueChanged += (oldVal, newVal) => _animator.SetFloat(_animIDSpeed, newVal);
        netGrounded.OnValueChanged += (oldVal, newVal) => _animator.SetBool(_animIDGrounded, newVal);
        netJump.OnValueChanged += (oldVal, newVal) => _animator.SetBool(_animIDJump, newVal);
        netFreeFall.OnValueChanged += (oldVal, newVal) => _animator.SetBool(_animIDFreeFall, newVal);
        netMotion.OnValueChanged += (oldVal, newVal) => _animator.SetFloat(_animIDMotionSpeed, newVal);
    }

    /// <summary>
    /// 오너만 자신의 애니메이션 상태를 네트워크로 전송
    /// </summary>
    public void SetAnimatorValues(float speed, bool grounded, bool jump, bool freeFall, float motion)
    {
        if (!IsOwner) return;

        netSpeed.Value = speed;
        netGrounded.Value = grounded;
        netJump.Value = jump;
        netFreeFall.Value = freeFall;
        netMotion.Value = motion;
    }
}

