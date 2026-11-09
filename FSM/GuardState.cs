using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardState : StateBase
{
    private Animator animator;
    public GuardState(Animator animator)
    {
        this.animator = animator;
    }
    public override void OnEnter()
    {

        animator.SetBool("isGuard", true);
    }
    public override void OnUpdate()
    {

    }
    public override void OnExit()
    {
        animator.SetBool("isGuard", false);
    }

}