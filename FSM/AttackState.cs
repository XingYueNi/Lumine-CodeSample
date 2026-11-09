using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AttackState : StateBase
{
    private Animator animator;
    public AttackState(Animator animator)
    {
        this.animator = animator;
    }
    public override void OnEnter()
    {
        animator.SetBool("isAttack", true);
    }
    public override void OnUpdate()
    {
        
    }
    public override void OnExit()
    {
        animator.SetBool("isAttack", false);
    }

}