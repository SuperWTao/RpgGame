using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Entity CombatEntity { get; private set; }
    protected Animator _anim;
    protected Rigidbody _rb;
    public NavMeshAgent agent;
    protected FSM _fsm;

    public float radius;
    public LayerMask playerLayer;
    public Player attackTarget;

    public float attackRange;
    
    // 动画参数
    private bool _walk;
    private bool _run;
    private bool _attack;
    private bool _dead;
    private bool _isHurt;
    public bool Walk
    {
        get { return _walk;}
        set
        {
            _walk = value;
            _anim.SetBool("Walk", value);
        }
    }
    public bool Run
    {
        get { return _run;}
        set
        {
            _run = value; 
            _anim.SetBool("Run", value);
            _anim.SetFloat("Critical", 0.8f);
        }
    }
    public bool Attack
    {
        get { return _attack;}
        set
        {
            _attack = value; 
            _anim.SetBool("Attack", value);
        }
    }
    public bool Hurt
    {
        get { return _isHurt;}
        set { _isHurt = value; }
    }
    
    // 数值
    public float speed;
    public int dam;
    public FloatNumeric moveSpeed = new FloatNumeric();
    public IntNumeric attackDam = new IntNumeric();
    

    protected virtual void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        _fsm = transform.AddComponent<FSM>();
        _fsm.InitFsm(this);

        moveSpeed.SetBase(speed);
        agent.speed = moveSpeed.Value;
        attackDam.SetBase(dam);
    }

    protected virtual void Start()
    {
        CombatEntity = new Entity();
        CombatEntity.Init();
        
    }

    private void OnEnable()
    {
        EventCenter.AddListener("PlayerDeadEvent", OnPlayerDead);
    }

    private void OnDisable()
    {
        EventCenter.RemoveListener("PlayerDeadEvent", OnPlayerDead);
    }

    public bool FoundPlayer()
    {
        Collider[] colliders = new Collider[2];
        int num = Physics.OverlapSphereNonAlloc (transform.position, radius, colliders, playerLayer, QueryTriggerInteraction.Ignore);
        if(num > 0)
            foreach(Collider coll in colliders)
            {
                attackTarget = coll.GetComponent<Player>();
                return true;
            }
        attackTarget = null;
        return false;
    }

    public void TakeDamage(DamageAction damage)
    {
        if (_dead)
            return;
        _anim.SetTrigger("Hit");
        damage.ApplyAction();
        // Debug.Log("当前血量" + CombatEntity.CurrentHealth.Value);
        
        if (CombatEntity.CurrentHealth.Value <= 0)
        {
            _dead = true;
            _anim.SetBool("Dead", true);
            _fsm.TransitionState(EnemyState.Dead);
            agent.isStopped = true;
            
            Destroy(gameObject, 2f);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    #region events

    private void OnPlayerDead(object obj)
    {
        agent.isStopped = true;
        Walk = false;
        Run = false;
        Attack = false;
        _fsm.TransitionState(EnemyState.Idle);
        _fsm.enabled = false;
    }

    #endregion
}
