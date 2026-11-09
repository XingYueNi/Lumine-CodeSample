using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : StateBase
{
    private Animator animator;
    public MoveState(Animator animator)
    {
        this.animator = animator;
    }
    public override void OnEnter()
    {
        animator.SetBool("isMove", true);
    }
    public override void OnUpdate()
    {

    }
    public override void OnExit()
    {
        animator.SetBool("isMove", false);
    }

}