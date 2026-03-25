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

    // 战斗系统
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
    private bool _attackInput;
    private bool _isRunning = false;
    private bool _isJumping = false;
    private bool _isGrounded;
    private float _groundCheckOffset = 0.5f;
    private float _fallMultiplier = 1.5f;

    private PlayerState _playerState;

    public Animator Anim => _anim;

#region 生命周期
    private void Awake()
    {
        _playerAction = new PlayerAction();
        _anim = GetComponent<Animator>();
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _characterController = GetComponent<CharacterController>();

        if (enableBattleRuntime)
        {
            InitBattleRuntime();
        }

        DontDestroyOnLoad(gameObject);
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
        // Battle tick 由 BattleWorld.Update 驱动，这里不再重复 AdvanceTick。

        CheckGround();
        SwitchPlayerState();
        CaculateGravity();
        Jump();
        Attack();
        CaculateInputDirection();
        Move();
        Rotate();

        HandleAttackHitWindow();
    }
#endregion

#region 输入回调
    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        _attackInput = true;
    }

    private void OnRunStarted(InputAction.CallbackContext ctx)
    {
        _isRunning = !_isRunning;
    }

    private void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        _isJumping = ctx.ReadValueAsButton();
    }
#endregion

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
            Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane hitPlane = new Plane(Vector3.up, transform.position);
            if (hitPlane.Raycast(ray, out float distance))
            {
                Vector3 targetPoint = ray.GetPoint(distance);
                transform.LookAt(targetPoint);
            }

            _anim.SetTrigger("Attack");
            _attackInput = false;
        }
    }

    private void HandleAttackHitWindow()
    {
        if (!enableBattleRuntime || _battle == null) return;

        if (!isAttacking)
        {
            _damageAppliedThisAttack = false;
            _hitEntityThisSwing.Clear();
            return;
        }

        if (_damageAppliedThisAttack) return;

        ApplyAttackHitByOverlap();
        _damageAppliedThisAttack = true;
    }

    private void ApplyAttackHitByOverlap()
    {
        if (_pipeline == null || _battle == null || _playerBattleEntity == null) return;
        if (_playerBattleEntity.IsDead) return;
        if (attackHitPoint == null) return;

        int count = Physics.OverlapSphereNonAlloc(
            attackHitPoint.position,
            attackHitRadius,
            _hitBuffer,
            enemyHitLayer,
            QueryTriggerInteraction.Ignore);
        Debug.Log(count);
        for (int i = 0; i < count; i++)
        {
            Collider col = _hitBuffer[i];
            if (col == null) continue;

            BattleEnemyActor enemyActor = col.GetComponentInParent<BattleEnemyActor>();
            if (enemyActor == null) continue;

            int targetId = enemyActor.EntityId;
            if (_hitEntityThisSwing.Contains(targetId)) continue;
            _hitEntityThisSwing.Add(targetId);

            if (!_battle.TryGetEntity(targetId, out var targetEntity)) continue;
            if (targetEntity.IsDead) continue;

            ActionRequest req = ActionRequest.CreateNormalAttack(NextRequestId(), playerEntityId, targetId);
            ActionResult result = _pipeline.Execute(_battle, req);

            if (!result.Success)
            {
                Debug.LogWarning("Player attack failed: " + result.Code + ", " + result.Message);
                continue;
            }

            Debug.Log("[Battle] Player hit Enemy(" + targetId + ") for " + result.DamageApplied
                      + ". Enemy HP: " + targetEntity.CurrentHp + "/" + targetEntity.MaxHp);

            if (targetEntity.IsDead)
            {
                Debug.Log("[Battle] Enemy Dead: " + targetId);
            }
        }
    }
#endregion

#region BattleRuntime
    private void InitBattleRuntime()
    {
        BattleWorld world = BattleWorld.Instance;
        if (world == null)
        {
            Debug.LogError("[Battle] BattleWorld.Instance is null. Please add BattleWorld to scene.");
            enableBattleRuntime = false;
            return;
        }

        _battle = world.Battle;
        _eventBus = world.EventBus;
        _effectRegistry = world.EffectRegistry;
        _pipeline = world.Pipeline;

        if (_battle == null || _eventBus == null || _effectRegistry == null || _pipeline == null)
        {
            Debug.LogError("[Battle] BattleWorld runtime is not initialized.");
            enableBattleRuntime = false;
            return;
        }

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

        _effectRegistry.AddPassive(playerEntityId, new ExecutionHealPassive(5));

        _battleEventHandler = evt =>
        {
            // Debug.Log("[BattleEvent] stage=" + evt.Stage + ", req=" + evt.RequestId + ", tick=" + evt.Tick);
        };
        _eventBus.SubscribeAll(_battleEventHandler);
    }

    private long NextRequestId()
    {
        BattleWorld world = BattleWorld.Instance;
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