using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitState : StateBase
{
    private Animator animator;
    public HitState(Animator animator)
    {
        this.animator = animator;
    }
    public override void OnEnter()
    {
        animator.SetBool("isHit", true);
    }
    public override void OnUpdate()
    {

    }
    public override void OnExit()
    {
        animator.SetBool("isHit", false);
    }

}