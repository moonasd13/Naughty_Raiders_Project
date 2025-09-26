using Define_Enums;
using StarterAssets;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerMove : NetworkBehaviour
{
    [SerializeField] public CharacterController _controller;
    [SerializeField] public Animator _animator;
    [SerializeField] public PlayerItem PlayerItem;
    [SerializeField] GameObject _gun;
    [SerializeField] Transform _rHPos;
    private GameObject _mainCamera;

    [Header("Player")]
    public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(2.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> SprintSpeed = new NetworkVariable<float>(5.335f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10.0f;
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    public float JumpTimeout = 0.50f;
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    public GameObject CinemachineCameraTarget;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private bool _hasAnimator;

    private float _defaultMoveSpeed;
    private float _defaultSprintSpeed;
    private float _speedIncrease = 1.3f;

    Item_Gun _item;


    [Header("NetworkVariable")]
    public NetworkVariable<float> AnimationBlend = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<float> NetworkMotionSpeed = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<ItemKind> my_ItemKind = new NetworkVariable<ItemKind>(ItemKind.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> equip = new NetworkVariable<bool>( false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> _isStun = new NetworkVariable<bool>( false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> inHand = new NetworkVariable<bool>( false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> in_action = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> hide = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        _hasAnimator = TryGetComponent(out _animator);
        AssignAnimationIDs();

        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    void Update()
    {
        if (!IsOwner) return;

        if (!_isStun.Value)
        {
            // 입력 수집
            float horizontal = 0f;
            float vertical = 0f;
            if (Input.GetKey(KeyCode.W)) vertical += 1f;
            if (Input.GetKey(KeyCode.S)) vertical -= 1f;
            if (Input.GetKey(KeyCode.D)) horizontal += 1f;
            if (Input.GetKey(KeyCode.A)) horizontal -= 1f;

            Vector2 moveInput = new Vector2(horizontal, vertical).normalized;
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            Move(moveInput, isSprinting);
            JumpAndGravity();

            /// 아이템 사용
            if (equip.Value && UnityEngine.Input.GetKeyDown(KeyCode.F))
            {
                PlayerItem.Useitem(this);
            }

            /// 상호작용
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                TryInteractWithNearbyBox();
            }
        }

        GroundedCheck();
    }

    private void LateUpdate()
    {
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, AnimationBlend.Value);
            _animator.SetFloat(_animIDMotionSpeed, NetworkMotionSpeed.Value);
        }

        if (!IsOwner)
        {
            return;
        }

        // Owner만 자신의 카메라를 회전시킵니다.
        CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    #region Movement, Jump & Camera
    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        if (_hasAnimator)
            _animator.SetBool(_animIDGrounded, Grounded);
    }

    private void CameraRotation()
    {
        if (!LockCameraPosition)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            float sensitivity = 1.0f;

            _cinemachineTargetYaw += mouseX * sensitivity;
            _cinemachineTargetPitch -= mouseY * sensitivity;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
            _cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw,
            0.0f
        );
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    private void Move(Vector2 moveInput, bool isSprinting)
    {
        float targetSpeed = (moveInput != Vector2.zero) ? (isSprinting ? SprintSpeed.Value : MoveSpeed.Value) : 0f;

        // 현재 속도
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else _speed = targetSpeed;

        // 애니메이션 블렌드
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        if (IsOwner)
        {
            UpdateAnimationBlendServerRpc(_animationBlend);
        }


        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (moveInput != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);

        if (IsOwner)
        {
            UpdateMotionSpeedServerRpc(moveInput.magnitude);
        }
    }

    #region[애니메이션 블렌드 + 모션스피드 동기화]
    [ServerRpc]
    private void UpdateAnimationBlendServerRpc(float blendValue)
    {
        AnimationBlend.Value = blendValue;
    }
    [ServerRpc]
    private void UpdateMotionSpeedServerRpc(float motionSpeed)
    {
        NetworkMotionSpeed.Value = motionSpeed;
    }
    #endregion

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            if (_verticalVelocity < 0f)
                _verticalVelocity = -2f;

            if (Input.GetKeyDown(KeyCode.Space) && _jumpTimeoutDelta <= 0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                if (_hasAnimator) _animator.SetBool(_animIDJump, true);
            }

            if (_jumpTimeoutDelta >= 0f) _jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;
            if (_fallTimeoutDelta >= 0f) _fallTimeoutDelta -= Time.deltaTime;
            else if (_hasAnimator) _animator.SetBool(_animIDFreeFall, true);
        }

        if (_verticalVelocity < _terminalVelocity)
            _verticalVelocity += Gravity * Time.deltaTime;
    }
    #endregion

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// 레이케스트를 이용한 상자찾기, 상자 상호작용
    /// </summary>
    private void TryInteractWithNearbyBox()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2.5f); // 범위 내 상자 찾기
        foreach (var hit in hits)
        {
            RoomItemBox box = hit.GetComponent<RoomItemBox>();
            Obj_Cabinet cabinet = hit.GetComponent<Obj_Cabinet>();
            Coin coin = hit.GetComponent<Coin>();
            ItemObject item = hit.GetComponent<ItemObject>();


            if (box != null)
            {
                box.SubmitInteractServerRpc();
                break;
            }

            if (cabinet != null)
            {
                cabinet.Interact(this);
                break;
            }
            
            if (coin != null)
            {
                coin.GetCoin(this);
                break;
            }

            if (item != null)
            {
                item.Getitem(this);
                break;
            }

        }
    }

    #region[액션 자동화]
    private void OnEnable()
    {
        in_action.OnValueChanged += OnActionValueChanged;
    }

    private void OnDisable()
    {
        in_action.OnValueChanged -= OnActionValueChanged;
    }

    private void OnActionValueChanged(bool previousValue, bool newValue)
    {
        if (!previousValue && newValue)
        {
            DoAction();
        }
        else if (previousValue && !newValue)
        {
            CancelAction();
        }
    }

    private void DoAction()
    {
        _animator.SetBool("Shooting", true);
    }

    private void CancelAction()
    {
        _animator.SetBool("Shooting", false);
    }


    /// 스피드 값 수정
    public void CjangeSpeed()
    {
        _defaultMoveSpeed = MoveSpeed.Value;
        _defaultSprintSpeed = SprintSpeed.Value;
        equip.Value = false;
        my_ItemKind.Value = ItemKind.None;

        MoveSpeed.Value *= _speedIncrease;
        SprintSpeed.Value *= _speedIncrease;

        StartCoroutine(RevertSpeedAfterDelay(10f));
    }
    private IEnumerator RevertSpeedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        MoveSpeed.Value = _defaultMoveSpeed;
        SprintSpeed.Value = _defaultSprintSpeed;
    }

    public void UseGun()
    {
        in_action.Value = true;
        ShootServerRpc();
    }

    /// 총 생성 RPC
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    void ShootServerRpc()
    {
        //Quaternion fireRotation = Quaternion.LookRotation(shootDir);

        GameObject itemObj = Instantiate(_gun, _rHPos.position, _rHPos.rotation);
        var netObj = itemObj.GetComponent<NetworkObject>();
        netObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        netObj.Spawn(true);

        _item = netObj.GetComponent<Item_Gun>();
        _item.equip = true;
    }

    // 슈팅 에니메이션 종료
    private void ShootingOff()
    {
        if (!IsOwner) return;
        ShootoffServerRpc();
    }

    [ServerRpc]
    public void ShootoffServerRpc()
    {
        in_action.Value = false;
        equip.Value = false;
        my_ItemKind.Value = ItemKind.None;

        Destroy(_item.gameObject);
    }

    /// <summary>
    /// 텔레포트
    /// </summary>
    /// <param name="animationEvent"></param>
    [ClientRpc]
    public void TeleportTargetClientRpc(Vector3 targetPos, Quaternion targetRot)
    {
        // 로직은 그대로
        var controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        transform.SetPositionAndRotation(targetPos, targetRot);

        StartCoroutine(ReenableControllerNextFrame(controller));
    }

    // PlayerMove.cs 내부에 정의
    private IEnumerator ReenableControllerNextFrame(CharacterController controller)
    {
        yield return null; // 한 프레임 대기
        if (controller != null) controller.enabled = true;
    }
    #endregion

    #region[발소리]
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            //if (FootstepAudioClips.Length > 0)
            //{
            //    var index = Random.Range(0, FootstepAudioClips.Length);
            //    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            //}
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        //if (animationEvent.animatorClipInfo.weight > 0.5f)
        //{
        //    AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        //}
    }
    #endregion
}
