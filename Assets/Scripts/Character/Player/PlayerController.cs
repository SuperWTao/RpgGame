using System;
using System.Collections;
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
    private static readonly int CACHE_SIZE = 3;
    private Vector3[] _velCache = new Vector3[CACHE_SIZE];
    private int _currentChacheIndex = 0;
    private Vector3 _averageVel = Vector3.zero;
    #endregion
    public Entity CombatEntity { get; private set; }
    // 组件
    private PlayerAction _playerAction;
    private Vector2 _inputValue => _playerAction.Player.Move.ReadValue<Vector2>();
    private Animator _anim;
    private Camera _camera;
    private CharacterController _characterController;

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
    private bool _dead;
    private bool _isRunning = false;
    private bool _isJumping = false;
    private bool _isGrounded;
    private float _groundCheckOffset = 0.5f;
    private float _fallMultiplier = 1.5f;
    
    private PlayerState _playerState;
    private float _groundMoveThreshold = 0f;
    private float _jumpThreshold = 1f;
    private float midairThreshold = 2.1f;
    private float landingThreshold = 1f;
    
    [Header("武器")]
    public GameObject weaponObj;
    private PlayerWeapon _weapon;


    public Animator Anim
    {
        get
        {
            return _anim;
        }
    }

    #region 生命周期
    protected virtual void Awake()
    {
        _playerAction = new PlayerAction();
        _anim = GetComponent<Animator>();
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _weapon = GetComponentInChildren<PlayerWeapon>();
        _characterController = GetComponent<CharacterController>();
        CombatEntity = new Entity();
        CombatEntity.Init();
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        CombatEntity.AddListener(ActionPointType.PostCauseDamage, OnPlayerAttack);
        CombatEntity.AddListener(ActionPointType.PostReceiveDamage, OnPlayerHit);
    }

    protected virtual void OnEnable()
    {
        _playerAction.Enable();
        // 攻击按键回调，用于检测攻击输入
        _playerAction.Player.Attack.performed += ctx =>
        {
            _attackInput = true;
        };
        _playerAction.Player.Run.started += ctx =>
        {
            _isRunning = !_isRunning;
        };
        _playerAction.Player.Jump.started += ctx =>
        {
            _isJumping = ctx.ReadValueAsButton();
        };
    }

    protected virtual void Update()
    {
        CheckGround();
        SwitchPlayerState();
        SetUpAnimator();
        CaculateGravity();
        Jump();
        Attack();
        CaculateInputDirection();
        Move(); 
        Rotate();
    }

    private void OnDisable()
    {
        _playerAction.Disable();
    }
    #endregion

    #region 人物移动跳跃
    private void CheckGround()
    {
        if(Physics.SphereCast(this.transform.position + (Vector3.up * _groundCheckOffset), _characterController.radius, Vector3.down, out RaycastHit hitInfo, _groundCheckOffset - _characterController.radius + 2 * _characterController.skinWidth))
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
            // 由于characterController的问题，需要一直施加向下的速度，否则无法检测到是否在地面上
            _verticalVelocity = _gravity * Time.deltaTime;
            return;
        }
        else
        {
            if (_verticalVelocity < 0 || !_isJumping)
            {
                _verticalVelocity += _gravity * Time.deltaTime * _fallMultiplier;
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }
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
        // 获取相机的前方在水平平面上的投影
        Vector3 camForwardProjection = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;
        // 得到世界坐标下的移动向量
        _playerMovement = camForwardProjection * _inputValue.y + _camera.transform.right * _inputValue.x;
        // 将世界坐标下的移动向量转换为局部坐标下的移动向量
        // _playerMovement = this.transform.InverseTransformVector(_playerMovement);
    }
    
    /// <summary>
    /// 通过输入控制玩家的旋转
    /// </summary>
    private void Rotate()
    {
        // Vector3 worldMove = transform.TransformDirection(_playerMovement);
        Vector3 worldMove = _playerMovement;
        worldMove.y = 0f;
        if (worldMove.sqrMagnitude < 0.0001f) return;
        Vector3 dir = worldMove.normalized;
        float forwardDot = Vector3.Dot(transform.forward, dir);
        // 如果是后退（与朝前方向夹角大于90度），则不直接转向背面
        if (forwardDot < 0f)
        {
            return;
        }
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
        _currentChacheIndex++;
        _currentChacheIndex %= CACHE_SIZE;
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

    public void Attack()
    {
        _anim.SetFloat("StateTime", Mathf.Repeat(_anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
        if (_attackInput)
        {
            Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane hitPlane = new Plane(Vector3.up, transform.position);
            float distance;
            if (hitPlane.Raycast(ray, out distance))
            {
                Vector3 targetPoint = ray.GetPoint(distance);
                transform.LookAt(targetPoint);
            }
            _anim.SetTrigger("Attack");
            _attackInput = false;
        }
    }

    private void SwitchPlayerState()
    {
        _playerState = !_isGrounded ? PlayerState.Jump : PlayerState.Move;
        _anim.SetFloat("PlayerState", (float)_playerState);
    }

    private void SetUpAnimator()
    {
        
    }
    

    // public void WeaponAttackStart()
    // {
    //     _weapon.StartAttack();
    // }
    //
    // public void WeaponAttackEnd()
    // {
    //     _weapon.EndAttack();
    // }

    #region unity_events

    private void OnPlayerAttack(CombatAction action)
    {
        // TODO:播放攻击特效，音效
        // Debug.Log("攻击");
    }
    
    private void OnPlayerHit(CombatAction obj)
    {
        // TODO:播放受击特效，音效
        Debug.Log($"玩家当前生命值为{CombatEntity.CurrentHealth.Value}");
        if (CombatEntity.CurrentHealth.Value <= 0)
        {
            _dead = true;
            EventCenter.TriggerEvent("PlayerDeadEvent");
            _anim.SetBool("Dead", _dead);
            _playerAction.Disable();
            return;
        }
        _anim.SetTrigger("Hit");
    }

    #endregion
}
