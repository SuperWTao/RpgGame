using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMeleeEffect : StateMachineBehaviour
{
    public int attackIndex;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player player = animator.GetComponent<Player>();
        // 可以加载对应的特效
        player.isAttacking = true;
        player.WeaponAttackStart();

        if (attackIndex == 3)
            player.isDashing = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player player = animator.GetComponent<Player>();
        player.isAttacking = false;
        player.WeaponAttackEnd();
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    // override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //     // Implement code that processes and affects root motion
    // }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    // override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //     // Implement code that sets up animation IK (inverse kinematics)
    // }
}
