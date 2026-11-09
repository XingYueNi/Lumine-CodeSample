using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : StateBase
{
    private Animator animator;
    public DeathState(Animator animator)
    {
        animator = this.animator;
    }
    public override void OnEnter()
    {
        animator.SetBool("isDeath", true);
    } 
    public override void OnUpdate()
    {

    }
    public override void OnExit()
    {
        animator.SetBool("isDeath", false);
    }

}
