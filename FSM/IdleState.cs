using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : StateBase
{
    private Animator animator;
    public IdleState(Animator animator)
    {
        this.animator = animator;
    }
    public override void OnEnter()
    {
        animator.SetBool("isIdle",true);
    }
    public override void OnUpdate()
    {
    }
    public override void OnExit()
    {
        animator.SetBool("isIdle", false);
    }

}
