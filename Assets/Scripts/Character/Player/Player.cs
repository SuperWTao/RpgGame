using System;
using System.Collections;
using System.Collections.Generic;
using cfg;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Entity CombatEntity { get; private set; }
    // 组件
    private PlayerInputControl _playerInputControl;
    private Vector2 _inputValue => _playerInputControl.keyboard.Move.ReadValue<Vector2>();
    private Rigidbody _rb;
    private Animator _anim;
    private Camera _camera;
    
    [Header("攻击移动")]
    public float walkSpeed;
    public float runSpeed;
    public float dashDistance = 1f;      // 冲刺距离
    
    [Header("状态")]
    public bool isAttacking;
    private bool _attackInput; // 接收攻击输入
    public bool isDashing;
    public bool hit;
    private bool _dead;

    private SwordWeapon _weapon;
    
    public float Speed
    {
        get
        {
            if (_playerInputControl.keyboard.Run.IsPressed() && _inputValue.y > 0)
                return runSpeed;
            else
                return walkSpeed;
        }
    }
    
    public float rotationSpeed;

    protected virtual void Awake()
    {
        _playerInputControl = new PlayerInputControl();
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _weapon = GetComponentInChildren<SwordWeapon>();
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
        _playerInputControl.Enable();
        // 攻击按键回调，用于检测攻击输入
        _playerInputControl.keyboard.Attack.performed += ctx =>
        {
            _attackInput = true;
        };
        _playerInputControl.keyboard.Attack.performed += ctx =>
        {
            // TODO: 按下空格释放技能逻辑
        };
    }

    protected virtual void FixedUpdate()
    {
        Move();
        Attack();
    }

    private void OnDisable()
    {
        _playerInputControl.Disable();
    }
    
    /// <summary>
    /// 玩家旋转
    /// </summary>
    public void Rotate()
    {
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 lookPos;
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            lookPos = hit.point;
        }
        else
        {
            lookPos = ray.origin + ray.direction * 50f;
        }
        
        lookPos.y = transform.position.y;
        Vector3 lookDir = (lookPos - transform.position).normalized;
        if (lookDir.sqrMagnitude > 0.1f)
        {
            Quaternion targetQuaternion = Quaternion.LookRotation(lookDir);
            transform.rotation =
                Quaternion.Lerp(transform.rotation, targetQuaternion, rotationSpeed * Time.deltaTime);
        }
    }
    /// <summary>
    /// 玩家移动
    /// 以面朝方向
    /// </summary>
    // public void Move()
    // {
    //     _anim.SetFloat("Forward", _inputValue.y);
    //     if (_inputValue != Vector2.zero && !isAttacking)
    //     {
    //         Vector3 moveDir = (transform.forward * _inputValue.y + transform.right * _inputValue.x).normalized;
    //         if (moveDir.magnitude > 0.1f)
    //         {
    //             _rb.MovePosition(_rb.position + moveDir * Speed * Time.fixedDeltaTime);
    //             // Vector3 targetDir = Vector3.Lerp(transform.forward, moveDir, Time.deltaTime * rotationSpeed);
    //             // transform.rotation = Quaternion.LookRotation(targetDir);
    //             _anim.SetFloat("Speed", Speed);
    //         }
    //     }
    //     else
    //     {
    //         _anim.SetFloat("Speed", 0);
    //     }
    // }
    
    /// <summary>
    /// 以世界坐标方向的人物移动
    /// </summary>
    public void Move()
    {
        Vector3 moveDir = new Vector3(-_inputValue.x, 0, -_inputValue.y).normalized;
        if (_inputValue != Vector2.zero && !isAttacking)
        {
            _rb.MovePosition(_rb.position + moveDir * Speed * Time.fixedDeltaTime);
            _anim.SetFloat("Speed", Speed);
            
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            // transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            transform.rotation = targetRot;
        }
        else
        {
            _anim.SetFloat("Speed", 0);
        }
    }

    public void Attack()
    {
        _anim.SetFloat("StateTime", Mathf.Repeat(_anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
        if (_attackInput)
        {
            Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Vector3 mousePos;
            if(Physics.Raycast(ray, out RaycastHit hit, 100f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                mousePos = hit.point;
            }
            else
            {
                mousePos = ray.origin + ray.direction * 50f;
            }
            Vector3 lookDir = (mousePos - transform.position).normalized;
            lookDir.y = 0;
            Quaternion targetQuaternion = Quaternion.LookRotation(lookDir);
            transform.rotation = targetQuaternion;
           _anim.SetTrigger("Attack");
           // if (isDashing)
           //     StartCoroutine(AttackDash(lookDir));
           _attackInput = false;
        }
    }
    
    /// <summary>
    /// 第三段攻击冲刺
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private IEnumerator AttackDash(Vector3 pos)
    {
        Vector3 currentPos = transform.position;
        float step = 0.1f;
        float remainingDistance = dashDistance;
        while (remainingDistance > 0.01f)
        {
            float moveDis = Mathf.Min(remainingDistance, step);
            if (Physics.Raycast(currentPos, pos, out RaycastHit hit, moveDis, 6))
            {
                _rb.MovePosition(hit.point);
                currentPos = hit.point;
            }
            currentPos += pos * moveDis;
            _rb.MovePosition(currentPos);
            remainingDistance -= moveDis;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;
    }

    public void WeaponAttackStart()
    {
        _weapon.StartAttack();
    }
    
    public void WeaponAttackEnd()
    {
        _weapon.EndAttack();
    }

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
            _playerInputControl.Disable();
            return;
        }
        _anim.SetTrigger("Hit");
    }

    #endregion
    
}
