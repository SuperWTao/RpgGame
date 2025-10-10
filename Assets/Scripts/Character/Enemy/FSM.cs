using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EnemyState
{
    Idle, // 待机
    Patrol, // 巡逻
    Chase, // 追击
    fury, // 狂暴
    Attack, // 攻击
    Dead // 死亡
}

public class FSM : MonoBehaviour
{
    private Istate _currentState;
    public Dictionary<EnemyState, Istate> states = new Dictionary<EnemyState, Istate>();
    public float idleTime = 1f;
    public Enemy enemy;

    public void InitFsm(Enemy enemy)
    {
        this.enemy = enemy;
    }

    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        _currentState.OnUpdate();
    }

    private void FixedUpdate()
    {
        _currentState.OnFixUpdate();
    }

    public void TransitionState(EnemyState type)
    {
        _currentState?.OnExit();
        _currentState = states[type];
        _currentState.OnEnter();
    }
}

public interface Istate
{
    void OnEnter();
    void OnUpdate();
    void OnFixUpdate();
    void OnExit();
}
