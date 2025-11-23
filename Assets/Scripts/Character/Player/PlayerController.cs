using System;
using System.Collections;
using System.Collections.Generic;
using cfg;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Entity CombatEntity { get; private set; }
    // 组件
    private PlayerAction _playerAction;
    private Vector2 _inputValue => _playerAction.Player.Move.ReadValue<Vector2>();
    private Animator _anim;
    private Camera _camera;
    private CharacterController _characterController;

    [Header("移动")]
    public float moveSpeed;
    private float _currentSpeed;
    private float _rotationSpeed = 1000f;
    
    [Header("状态")]
    public bool isAttacking;
    private bool _attackInput;
    private bool _dead;
    
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
    }

    protected virtual void FixedUpdate()
    {
        Attack();
        Move(); 
    }

    private void Update()
    {
        Rotate();
    }

    private void OnDisable()
    {
        _playerAction.Disable();
    }
    
    /// <summary>
    /// 通过输入控制玩家的旋转
    /// </summary>
    private void Rotate()
    {
        if(_inputValue == Vector2.zero) return;
        Vector3 dir;
        dir = new Vector3(_inputValue.x, 0, _inputValue.y);
        transform.InverseTransformDirection(dir);
        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private void Move()
    {
        if (_inputValue == Vector2.zero || isAttacking)
        {
            _anim.SetFloat("ScaleFactor", 1/_anim.humanScale);
            _anim.SetFloat("Speed", 0);
            return;
        }
        _anim.SetFloat("ScaleFactor", 1 / _anim.humanScale * moveSpeed);
        float targetSpeed = 1;
        targetSpeed *= _inputValue.magnitude;
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, 0.5f);
        _anim.SetFloat("Speed", _currentSpeed);
    }

    private void OnAnimatorMove()
    {
        // 控制玩家在动画中的位移和旋转起作用
        _characterController.SimpleMove(_anim.velocity);
        transform.Rotate(_anim.deltaRotation.eulerAngles);
    }


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
