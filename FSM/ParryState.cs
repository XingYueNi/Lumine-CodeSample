using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParryState : StateBase
{
    private Animator animator;
    public ParryState(Animator animator)
    {
        this.animator = animator;
    }
    public override void OnEnter()
    {
        animator.SetBool("isParry", true);
    }
    public override void OnUpdate()
    {

    }
    public override void OnExit()
    {
        animator.SetBool("isParry", false);
    }

}