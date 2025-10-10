using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordWeapon : MonoBehaviour
{
    // 武器上位置的偏移
    [System.Serializable]
    public struct AttackPoint
    {
        public float radius;
        public Vector3 offset;
        public Transform attackRoot;
// #if UNITY_EDITOR
//         // 用于绘制轨迹
//         [NonSerialized] public List<Vector3> previousPositions;
// #endif

    }
    
    public AttackPoint[] attackPoints; 
    private Vector3[] _previousPos; // 上一帧的位置
    public LayerMask targetLayer;
    public bool isAttacking;
    public Player owner;
    
    private RaycastHit[] _raycastHitCache = new RaycastHit[32];
    private HashSet<Collider> _colliderSet = new HashSet<Collider>();

    private void Start()
    {
        owner = GetComponentInParent<Player>();
    }

    private void Update()
    {
        if(isAttacking)
            FindEnemy();
    }

    public void FindEnemy()
    {
        for (int i = 0; i < attackPoints.Length; i++)
        {
            AttackPoint pos = attackPoints[i];
            Vector3 wordPos = pos.attackRoot.position + pos.attackRoot.TransformVector(pos.offset);
            Vector3 attackVec = wordPos - _previousPos[i]; // 获取上一帧和当前帧的位置差
            if (attackVec.magnitude < 0.0001f) // 防止第一帧的向量为0
            {
                attackVec = Vector3.forward * 0.0001f;
            }

            Ray ray = new Ray(_previousPos[i], attackVec.normalized); // 创建射线，从上一帧的位置移动往当前帧的方向移动
            int contacts = Physics.SphereCastNonAlloc(ray, pos.radius, _raycastHitCache, attackVec.magnitude,
                targetLayer, QueryTriggerInteraction.Ignore);

            for (int j = 0; j < contacts; j++)
            {
                Collider col = _raycastHitCache[j].collider;
                if(_colliderSet.Contains(col))
                    continue;
                _colliderSet.Add(col);
                // TODO: 施加伤害, 后续需要进行修改，先固定伤害进行测试
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    var damage = new DamageAction(ActionType.Damage, owner.CombatEntity, enemy.CombatEntity, 10);
                    enemy.TakeDamage(damage);
                }
            }
            _previousPos[i] = wordPos;
// #if UNITY_EDITOR
//             pos.previousPositions.Add(_previousPos[i]);
// #endif
        }
    }
    

    public void StartAttack()
    {
        isAttacking = true;
        _previousPos = new Vector3[attackPoints.Length];
        for (int i = 0; i < attackPoints.Length; i++)
        {
            Vector3 wordPos = attackPoints[i].attackRoot.position +
                              attackPoints[i].attackRoot.TransformVector(attackPoints[i].offset);
            _previousPos[i] = wordPos;
// #if UNITY_EDITOR
//             attackPoints[i].previousPositions.Clear();
//             attackPoints[i].previousPositions.Add(wordPos);
// #endif
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        _colliderSet.Clear();
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < attackPoints.Length; i++)
        {
            AttackPoint pos = attackPoints[i];
            if (pos.attackRoot != null)
            {
                Vector3 worldPos = pos.attackRoot.TransformVector(pos.offset);
                Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
                Gizmos.DrawSphere(pos.attackRoot.position + worldPos, pos.radius);

                // if (pos.previousPositions.Count > 1)
                // {
                //     UnityEditor.Handles.DrawAAPolyLine(10, pos.previousPositions.ToArray());
                // }
            }
        }
    }
#endif
}
