using System.Globalization;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.Windows;
using UnityEngine.Rendering.Universal;
using Unity.Netcode.Components;
using UnityEditor.Rendering;




#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class PlayerController : NetworkBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("pos")]
        public Transform righthandTransform;
        public Transform lefthandTransform;
        public NetworkVariable<bool> equip = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> inHand = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> in_action = new NetworkVariable<bool>(
           false,
           NetworkVariableReadPermission.Everyone,
           NetworkVariableWritePermission.Server);
        public GameObject[] item_List;
        public ItemObject Item;

        // cinemachine
        [SerializeField] public GameObject cinemachine_CM;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
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

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;
        // 서버에 보낼값
        private Vector2 _serverInputMove;
        private bool _serverInputSprint;

        private bool _hasAnimator;
        public bool _isStun { get; set; } = false;


        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            if (cinemachine_CM != null)
            {
                if (IsOwner)
                {
                    cinemachine_CM.SetActive(true);
                }
            }

            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();

            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            if (!IsSpawned) return;

            if (!_hasAnimator)
                _hasAnimator = TryGetComponent(out _animator);

            if (in_action.Value) return;

            GroundedCheck();

            // Owner 전용 처리
            if (IsOwner)
            {
                if (IsClient)
                {
                    MoveServerRpc(_input.move, _input.sprint, _input.jump);
                    _input.jump = false;
                }

                if (!_isStun && equip.Value && UnityEngine.Input.GetKeyDown(KeyCode.F))
                    ShootiongaAnimation();

                if (!_isStun && UnityEngine.Input.GetKeyDown(KeyCode.E))
                    TryInteractWithNearbyBox();
            }

            // Server 전용 처리
            if (IsServer && !_isStun)
            {
                _input.move = _serverInputMove;
                _input.sprint = _serverInputSprint;
                JumpAndGravity();
                Move();
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                cinemachine_CM?.SetActive(true);
            }

            // 클라이언트가 소유하지 않은 경우 입력 비활성화
            if (!IsOwner)
            {
                if (TryGetComponent(out PlayerInput input))
                    input.enabled = false;

                if (TryGetComponent(out StarterAssetsInputs sai))
                    sai.enabled = false;
            }
        }


        private void LateUpdate()
        {
            if (IsOwner)
            {
                float yawInput = _input.look.x;

                if (Mathf.Abs(yawInput) > 0.01f)
                {
                    float yawDelta = yawInput * RotationSpeed * (IsCurrentDeviceMouse ? 1f : Time.deltaTime);
                    RotateServerRpc(yawDelta);
                }

                CameraRotation(); // 클라용 Pitch 조정 (시각용)
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            DebugUIManager.Instance?.Log("GroundedCheck() called by: " + OwnerClientId);
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        /// <summary>
        /// 카메라 회전
        /// </summary>
        private void CameraRotation()
        {
            // if there is an input
            if (_input.look.sqrMagnitude >= _threshold)
            {
                //Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                // 마우스의 위로 움직이면 위를 보고 아래로 움직이면 아래를 본다.
                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

                // clamp our pitch rotation
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                // Update Cinemachine camera target pitch
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        /// <summary>
        /// 움직임 서버 RPC
        /// </summary>
        /// <param name="move"></param>
        /// <param name="sprint"></param>
        [ServerRpc]
        void MoveServerRpc(Vector2 move, bool sprint, bool jump)
        {
            _serverInputMove = move;
            _serverInputSprint = sprint;

            if (jump && _jumpTimeoutDelta <= 0.0f && Grounded)
            {
                DoJump();
            }
        }

        /// <summary>
        /// 움직임
        /// </summary>
        private void Move()
        {
            DebugUIManager.Instance?.Log("Move() called by: " + OwnerClientId);
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                // move
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
            }

            // move the player
            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // inputMagnitude 계산 후에 블렌딩 업데이트
            inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        /// <summary>
        /// 점프
        /// </summary>
        private void JumpAndGravity()
        {
            DebugUIManager.Instance?.Log("JumpAndGravity() called by: " + OwnerClientId);
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                //// Jump
                //if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                //{
                //    // the square root of H * -2 * G = how much velocity needed to reach desired height
                //    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                //    // update animator if using character
                //    if (_hasAnimator)
                //    {
                //        _animator.SetBool(_animIDJump, true);
                //    }
                //}

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        /// <summary>
        /// 서버 점프 처리
        /// </summary>
        private void DoJump()
        {
            _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

            // 애니메이션 처리
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, true);
            }

            _jumpTimeoutDelta = JumpTimeout;  // 점프 딜레이 설정
        }


        /// <summary>
        /// 카메라 회전 RPC
        /// </summary>
        /// <param name="yawDelta"></param>
        [ServerRpc]
        void RotateServerRpc(float yawDelta)
        {
            transform.Rotate(Vector3.up * yawDelta);
        }

        /// <summary>
        /// 카메라 각도
        /// </summary>
        /// <param name="lfAngle"></param>
        /// <param name="lfMin"></param>
        /// <param name="lfMax"></param>
        /// <returns></returns>
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        /// <summary>
        /// 테이져건 발사
        /// </summary>
        public void ShootiongaAnimation()
        {
            if (equip.Value == true)
            {
                RequestStartActionServerRpc();
            }
        }
        /// 발사애니메이션 RPC
        [ServerRpc]
        private void RequestStartActionServerRpc()
        {
            in_action.Value = true;
            _animator.SetBool("Shooting", true);
        }

        /// <summary>
        /// 스턴 작용
        /// </summary>
        public void stunON()
        {
            _isStun = true;
            StartCoroutine(StunTimer());
            if(IsOwner)
            Debug.Log("스턴걸림");
        }

        /// <summary>
        /// 스턴 코루틴
        /// </summary>
        /// <returns></returns>
        private IEnumerator StunTimer()
        {
            yield return new WaitForSeconds(3f);
            if (IsOwner)
                Debug.Log("스턴풀림");
            _isStun = false;
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
                if (box != null)
                {
                    box.SubmitInteractServerRpc(OwnerClientId);
                    break;
                }
            }
        }
        #region[애니메이션 이벤트]
        // 탄환 발사
        private void Shoot()
        {
            if (!IsOwner) return;
            Vector3 shootDir = Camera.main.transform.forward;
            ShootServerRpc(shootDir);
        }
        /// <summary>
        /// 총 생성 RPC
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        void ShootServerRpc(Vector3 shootDir)
        {
            Quaternion fireRotation = Quaternion.LookRotation(shootDir);
            fireRotation.z = 0;
            fireRotation.x = 0;

            GameObject itemObj = Instantiate(item_List[0], righthandTransform.position, fireRotation);
            var netObj = itemObj.GetComponent<NetworkObject>();
            netObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            netObj.Spawn(true);

            Item = netObj.GetComponent<ItemObject>();
            Item.FireServerRpc(shootDir);
        }



        // 테이져건 종료
        private void ShootingOff()
        {
            if (!IsOwner) return;
            ShootingOffServerRpc();
        }

        /// <summary>
        ///  테이져건 종료 RPC
        /// </summary>
        /// <param name="animationEvent"></param>
        [ServerRpc]
        void ShootingOffServerRpc()
        {
     
                in_action.Value = false;
                equip.Value = false;
                _animator.SetBool("Shooting", false);
                Item.gameObject.GetComponent<NetworkObject>().Despawn();
                Item = null;
        }



        //발소리
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        //지면착지
        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
        #endregion
    }
}
