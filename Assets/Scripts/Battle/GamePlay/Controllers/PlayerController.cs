using System;
using System.Collections.Generic;
using cfg;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Move = 0,
    Jump = 1
}

public class PlayerController : MonoBehaviour
{
#region 速度缓存池定义
    private const int CACHE_SIZE = 3;
    private readonly Vector3[] _velCache = new Vector3[CACHE_SIZE];
    private int _currentChacheIndex = 0;
    private Vector3 _averageVel = Vector3.zero;
#endregion

    // 输入与组件
    private PlayerAction _playerAction;
    private Vector2 _inputValue => _playerAction.Player.Move.ReadValue<Vector2>();
    private Animator _anim;
    private Camera _camera;
    private CharacterController _characterController;

    [Header("Battle Runtime")]
    [SerializeField] private bool enableBattleRuntime = true;
    [SerializeField] private int playerEntityId = 1;
    [SerializeField] private int playerMaxHp = 100;
    [SerializeField] private int playerAttack = 15;
    [SerializeField] private int playerDefense = 3;

    [Header("Hit Detection")]
    [SerializeField] private Transform attackHitPoint;
    [SerializeField] private float attackHitRadius = 1.4f;
    [SerializeField] private LayerMask enemyHitLayer;

    // 命中缓冲和去重
    private readonly Collider[] _hitBuffer = new Collider[32];
    private readonly HashSet<int> _hitEntityThisSwing = new HashSet<int>();
    private bool _damageAppliedThisAttack = false;

    [Header("战斗系统")]
    private int _currentAttackIndex = -1;
    private readonly List<int> _capturedTargetIds = new List<int>(16);
    private BattleContext _battle;
    private ICombatEventBus _eventBus;
    private BattleEffectRegistry _effectRegistry;
    private IBattlePipeline _pipeline;
    private BattleEntity _playerBattleEntity;

    private long _localRequestIdSeed = 10000;
    private Action<CombatEvent> _battleEventHandler;

    [Header("移动")]
    private Vector3 _playerMovement = Vector3.zero;
    public float walkSpeed;
    public float runSpeed;
    private float _targetSpeed;
    private float _currentSpeed;
    private float _rotationSpeed = 1000f;
    private float _gravity = -9.81f;
    private float _verticalVelocity;
    private float _maxHeight = 2f;

    [Header("状态")]
    public bool isAttacking;
    private bool _isControlLocked = false;
    private bool _attackInput;
    private bool _isRunning = false;
    private bool _isJumping = false;
    private bool _isGrounded;
    private float _groundCheckOffset = 0.5f;
    private float _fallMultiplier = 1.5f;

    private PlayerState _playerState;

    public Animator anim => _anim;

#region 生命周期
    private void Awake()
    {
        _playerAction = new PlayerAction();
        _anim = GetComponent<Animator>();
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _characterController = GetComponent<CharacterController>();

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (enableBattleRuntime)
        {
            InitBattleRuntime();
        }
    }

    private void OnEnable()
    {
        _playerAction.Enable();
        _playerAction.Player.Attack.performed += OnAttackPerformed;
        _playerAction.Player.Run.started += OnRunStarted;
        _playerAction.Player.Jump.started += OnJumpStarted;
    }

    private void OnDisable()
    {
        _playerAction.Player.Attack.performed -= OnAttackPerformed;
        _playerAction.Player.Run.started -= OnRunStarted;
        _playerAction.Player.Jump.started -= OnJumpStarted;
        _playerAction.Disable();

        if (_eventBus != null && _battleEventHandler != null)
        {
            _eventBus.UnsubscribeAll(_battleEventHandler);
        }
    }

    private void Update()
    {
        CheckGround();
        SwitchPlayerState();
        CaculateGravity();
        UpdateVerticalVelocityAnimParam();

        if (_isControlLocked)
        {
            _anim.SetFloat("Speed", 0f);
            return;
        }

        Jump();
        Attack();
        CaculateInputDirection();
        Move();
        Rotate();
    }
#endregion

#region 输入回调
    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (_isControlLocked) return;
        _attackInput = true;
    }

    private void OnRunStarted(InputAction.CallbackContext ctx)
    {
        if (_isControlLocked) return;
        _isRunning = !_isRunning;
    }

    private void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        if (_isControlLocked) return;
        // 空中按跳跃键不缓存输入，避免落地后触发二次起跳。
        if (!_isGrounded) return;
        _isJumping = ctx.ReadValueAsButton();
    }
#endregion

    public void SetControlLocked(bool locked)
    {
        _isControlLocked = locked;
        if (!locked) return;

        _attackInput = false;
        _isJumping = false;
        _isRunning = false;
        _targetSpeed = 0f;
        _currentSpeed = 0f;
        _anim.SetFloat("Speed", 0f);
    }

#region 移动跳跃
    private void CheckGround()
    {
        if (Physics.SphereCast(
                transform.position + (Vector3.up * _groundCheckOffset),
                _characterController.radius,
                Vector3.down,
                out _,
                _groundCheckOffset - _characterController.radius + 2 * _characterController.skinWidth))
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
            // 离地后清理跳跃输入，防止旧输入在落地后被消费。
            _isJumping = false;
        }
    }

    private void CaculateGravity()
    {
        if (_isGrounded)
        {
            _verticalVelocity = _gravity * Time.deltaTime;
            return;
        }

        if (_verticalVelocity < 0 || !_isJumping)
        {
            _verticalVelocity += _gravity * Time.deltaTime * _fallMultiplier;
        }
        else
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
    }

    private void Jump()
    {
        if (_isGrounded && _isJumping)
        {
            _verticalVelocity = Mathf.Sqrt(-2f * _gravity * _maxHeight);
            float feet = UnityEngine.Random.Range(-1f, 1f);
            _anim.SetFloat("LeftRightFeet", feet);
            _isJumping = false;
        }
    }

    private void UpdateVerticalVelocityAnimParam()
    {
        // Grounded 时将参数压到 0，避免落地后动画仍保持负值。
        float verticalForAnim = _isGrounded ? 0f : _verticalVelocity;
        _anim.SetFloat("VerticalVelocity", verticalForAnim);
    }

    private void CaculateInputDirection()
    {
        Vector3 camForwardProjection = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;
        _playerMovement = camForwardProjection * _inputValue.y + _camera.transform.right * _inputValue.x;
    }

    private void Rotate()
    {
        Vector3 worldMove = _playerMovement;
        worldMove.y = 0f;
        if (worldMove.sqrMagnitude < 0.0001f) return;

        Vector3 dir = worldMove.normalized;
        float forwardDot = Vector3.Dot(transform.forward, dir);
        if (forwardDot < 0f) return;

        Quaternion targetRotation = Quaternion.LookRotation(worldMove, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private void Move()
    {
        if (isAttacking)
        {
            _anim.SetFloat("Speed", 0);
            return;
        }

        if (_inputValue.y < 0)
        {
            _targetSpeed = -walkSpeed;
        }
        else
        {
            _targetSpeed = _isRunning ? runSpeed : walkSpeed;
        }

        _targetSpeed *= _inputValue.magnitude;
        _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, 0.5f);
        _anim.SetFloat("Speed", _currentSpeed);
    }

    private Vector3 AverageVelocity(Vector3 newVal)
    {
        _velCache[_currentChacheIndex] = newVal;
        _currentChacheIndex = (_currentChacheIndex + 1) % CACHE_SIZE;

        Vector3 averageVal = Vector3.zero;
        for (int i = 0; i < CACHE_SIZE; i++)
        {
            averageVal += _velCache[i];
        }

        return averageVal / CACHE_SIZE;
    }

    private void OnAnimatorMove()
    {
        if (_playerState != PlayerState.Move)
        {
            _averageVel.y = _verticalVelocity;
            Vector3 playerMoveDeltaMovement = _averageVel * Time.deltaTime;
            _characterController.Move(playerMoveDeltaMovement);
        }
        else
        {
            Vector3 playerMoveDeltaMovement = _anim.deltaPosition;
            playerMoveDeltaMovement.y = _verticalVelocity * Time.deltaTime;
            _characterController.Move(playerMoveDeltaMovement);
            transform.Rotate(_anim.deltaRotation.eulerAngles);
            _averageVel = AverageVelocity(_anim.velocity);
        }
    }
#endregion

#region 攻击逻辑
    public void Attack()
    {
        _anim.SetFloat("StateTime", Mathf.Repeat(_anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
        if (_attackInput)
        {   
            // 攻击朝向由动画控制，这里不再处理朝向。后续如果需要根据鼠标位置调整朝向，可以在这里添加代码，例如：
            // Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            // Plane hitPlane = new Plane(Vector3.up, transform.position);
            // if (hitPlane.Raycast(ray, out float distance))
            // {
            //     Vector3 targetPoint = ray.GetPoint(distance);
            //     transform.LookAt(targetPoint);
            // }

            _anim.SetTrigger("Attack");
            _attackInput = false;
        }
    }

    public void EnterAttackSegment(int attackIndex)
    {
        isAttacking = true;

        if (_currentAttackIndex != attackIndex)
        {
            _currentAttackIndex = attackIndex;
            _damageAppliedThisAttack = false;
            _hitEntityThisSwing.Clear();
            CaptureTargetsAtAttackStart();
        }
    }

    public void ExitAttackSegment(int attackIndex)
    {
        if (_currentAttackIndex != attackIndex) return;

        isAttacking = false;
        _currentAttackIndex = -1;
        _damageAppliedThisAttack = false;
        _hitEntityThisSwing.Clear();
        _capturedTargetIds.Clear();
    }

    // 由动画事件在命中帧触发，立即结算伤害。
    public void OnAttackHitFrameEvent()
    {
        if (!isAttacking) return;
        if (_currentAttackIndex < 0) return;

        // 进入攻击段时可能刚好没扫到目标；命中帧再补一次采样可显著降低漏判。
        if (_capturedTargetIds.Count == 0)
        {
            CaptureTargetsAtAttackStart();
        }

        ApplyCapturedDamageNow();
    }
    
    private void CaptureTargetsAtAttackStart()
    {
        _capturedTargetIds.Clear();

        if (_playerBattleEntity.isDead) return;
        if (attackHitPoint == null) return;

        int count = Physics.OverlapSphereNonAlloc(
            attackHitPoint.position,
            attackHitRadius,
            _hitBuffer,
            enemyHitLayer,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider col = _hitBuffer[i];
            if (col == null) continue;

            BattleEnemyActor enemyActor = col.GetComponentInParent<BattleEnemyActor>();
            if (enemyActor == null) continue;

            int targetId = enemyActor.battleEntityId;
            if (_capturedTargetIds.Contains(targetId)) continue;
            if (!_battle.TryGetEntity(targetId, out var targetEntity)) continue;
            if (targetEntity.isDead) continue;

            _capturedTargetIds.Add(targetId);
        }
    }

    private void ApplyCapturedDamageNow()
    {
        if (_damageAppliedThisAttack)
        {
            return;
        }
        if (_playerBattleEntity.isDead) return;

        for (int i = 0; i < _capturedTargetIds.Count; i++)
        {
            int targetId = _capturedTargetIds[i];

            if (!_battle.TryGetEntity(targetId, out var targetEntity)) continue;
            if (targetEntity.isDead) continue;

            ActionRequest req = ActionRequest.CreateNormalAttack(NextRequestId(), playerEntityId, targetId);
            ActionResult result = _pipeline.Execute(_battle, req);

            if (!result.success)
            {
                Debug.LogWarning("Player attack failed: " + result.code + ", " + result.message);
                continue;
            }

            Debug.Log("[Battle] Player hit Enemy(" + targetId + ") for " + result.damageApplied
                      + ". Enemy HP: " + targetEntity.currentHp + "/" + targetEntity.maxHp);

            if (targetEntity.isDead)
            {
                Debug.Log("[Battle] Enemy Dead: " + targetId);
            }
        }

        _damageAppliedThisAttack = true;
    }
#endregion

#region BattleRuntime
    private void InitBattleRuntime()
    {
        BattleWorld world = BattleWorld.instance;

        _battle = world.battle;
        _eventBus = world.eventBus;
        _effectRegistry = world.effectRegistry;
        _pipeline = world.pipeline;

        world.RegisterPlayer(
            playerEntityId,
            "Player",
            playerMaxHp,
            playerAttack,
            playerDefense,
            transform);

        if (!_battle.TryGetEntity(playerEntityId, out _playerBattleEntity))
        {
            Debug.LogError("[Battle] RegisterPlayer succeeded but player entity not found.");
            enableBattleRuntime = false;
            return;
        }

        _battleEventHandler = evt =>
        {
            // Debug.Log("[BattleEvent] stage=" + evt.stage + ", req=" + evt.requestId + ", tick=" + evt.tick);
        };
        _eventBus.SubscribeAll(_battleEventHandler);
    }

    private long NextRequestId()
    {
        BattleWorld world = BattleWorld.instance;
        if (world != null)
        {
            return world.NextRequestId();
        }

        _localRequestIdSeed++;
        return _localRequestIdSeed;
    }
#endregion

    private void SwitchPlayerState()
    {
        _playerState = !_isGrounded ? PlayerState.Jump : PlayerState.Move;
        _anim.SetFloat("PlayerState", (float)_playerState);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (attackHitPoint == null) return;
        Gizmos.color = new UnityEngine.Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawSphere(attackHitPoint.position, attackHitRadius);
    }
#endif
}