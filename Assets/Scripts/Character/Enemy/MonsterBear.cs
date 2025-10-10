using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBear : Enemy
{
    
    protected override void Awake()
    {
        base.Awake();
        attackRange = 2.0f;
    }

    protected override void Start()
    {
        base.Start();
        print(">>>>>>>>>>>>>>FSM>>>>>>>>>>>>>>");
        _fsm.states.Add(EnemyState.Idle, new BearIdleState(_fsm));
        _fsm.states.Add(EnemyState.Chase, new BearChaseState(_fsm));
        _fsm.states.Add(EnemyState.Attack, new BearAttackState(_fsm));
        _fsm.states.Add(EnemyState.Dead, new BearDeathState(_fsm));
        _fsm.TransitionState(EnemyState.Idle);
        
        CombatEntity.AddListener(ActionPointType.PostCauseDamage, OnBearAttack);
        CombatEntity.AddListener(ActionPointType.PostReceiveDamage, OnBearHit);
    }

    #region events

    private void OnBearHit(CombatAction obj)
    {
        Vector3 kickDir = (transform.position - attackTarget.transform.position).normalized;
        StartCoroutine(HitKnockBack(kickDir));
        
        if (CombatEntity.CurrentHealth.GetPercentage() <= 0.3 && !Run)
        {
            Run = true;
            // 这样使用并不好，只能添加buff不能移除buff，更好的方法是使用一个字典将buff存起来，但是因为敌人没有移除buff的需求，所以暂时不修改
            moveSpeed.PctAddModifier(new FloatModifier(20));
            agent.speed = moveSpeed.Value;
            attackDam.AddModifier(new IntModifier(5));
        }
    }

    private void OnBearAttack(CombatAction obj)
    {
        // TODO: 造成伤害后事件
    }

    #endregion

    
    private IEnumerator HitKnockBack(Vector3 kickDir)
    {
        Hurt = true;
        _rb.isKinematic = false;
        _rb.velocity = kickDir * 2f;
        yield return new WaitForSeconds(0.5f);
        _rb.velocity = Vector3.zero;
        _rb.isKinematic = true;
        Hurt = false;
    }

    public void HitPlayer()
    {
        if (!attackTarget)
            return;
        var damage = new DamageAction(ActionType.Damage, CombatEntity, attackTarget.CombatEntity, attackDam.Value);
        damage.ApplyAction();
    }
}

public class BearIdleState : Istate
{
    private FSM _manager;
    public float timer;
    
    public BearIdleState(FSM manager)
    {
        this._manager = manager;
    }
    public void OnEnter()
    {
        timer = 0f;
        // Debug.Log("Bear Idle State OnEnter");
    }

    public void OnUpdate()
    {
        timer += Time.deltaTime;
        if (timer > _manager.idleTime)
        {
            _manager.TransitionState(EnemyState.Chase);
        }
    }

    public void OnFixUpdate()
    {
        
    }

    public void OnExit()
    {
        timer = 0;
    }
}

public class BearChaseState : Istate
{
    private FSM _manager;
    
    public BearChaseState(FSM manager)
    {
        this._manager = manager;
    }
    public void OnEnter()
    {
        _manager.enemy.Walk = true;
        _manager.enemy.agent.isStopped = false;
        // Debug.Log("Bear Chase State OnEnter");
    }

    public void OnUpdate()
    {
        // 追击人物
        if (_manager.enemy.FoundPlayer() && !_manager.enemy.Hurt)
        {
            float distance = Vector3.Distance(_manager.enemy.transform.position, _manager.enemy.attackTarget.transform.position);
            _manager.enemy.agent.SetDestination(_manager.enemy.attackTarget.transform.position);
            _manager.enemy.agent.updateRotation = true;
            Vector3 toTarget = (_manager.enemy.attackTarget.transform.position - _manager.enemy.transform.position).normalized;
            float angle = Vector3.Angle(_manager.enemy.transform.forward, toTarget);
            if(distance <= _manager.enemy.attackRange && angle <= 90f)
            {
                _manager.TransitionState(EnemyState.Attack);
            }
        }

    }

    public void OnFixUpdate()
    {
        // 移动逻辑
    }

    public void OnExit()
    {
        _manager.enemy.Walk = false;
    }
}

public class BearAttackState : Istate
{
    private FSM _manager;
    public BearAttackState(FSM manager)
    {
        this._manager = manager;
    }
    public void OnEnter()
    {
        _manager.enemy.Attack = true;
        _manager.enemy.agent.isStopped = true;
        // Debug.Log("Bear Attack State OnEnter");
    }

    public void OnUpdate()
    {
        float distance = Vector3.Distance(_manager.enemy.transform.position, _manager.enemy.attackTarget.transform.position);
        Vector3 toTarget = (_manager.enemy.attackTarget.transform.position - _manager.enemy.transform.position).normalized;
        float angle = Vector3.Angle(_manager.enemy.transform.forward, toTarget);
        RotateTowardsTarget(toTarget);
        if(distance > _manager.enemy.attackRange && !_manager.enemy.Hurt || angle > 90f)
        {
            _manager.TransitionState(EnemyState.Chase);
        }
    }
    
    private void RotateTowardsTarget(Vector3 direction)
    {
        Vector3 flatDir = new Vector3(direction.x, 0, direction.z).normalized;
        if(flatDir.magnitude < 0.01f)
            return;
        Quaternion targetRotation = Quaternion.LookRotation(flatDir);
        _manager.enemy.transform.rotation =
            Quaternion.Slerp(_manager.enemy.transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public void OnFixUpdate()
    {
        
    }

    public void OnExit()
    {
        _manager.enemy.Attack = false;
    }
}

public class BearDeathState : Istate
{
    private FSM _manager;
    public BearDeathState(FSM manager)
    {
        this._manager = manager;
    }
    public void OnEnter()
    {
        // Debug.Log("Bear Death State OnEnter");
    }

    public void OnUpdate()
    {
        
    }

    public void OnFixUpdate()
    {
        
    }

    public void OnExit()
    {
        
    }
}
