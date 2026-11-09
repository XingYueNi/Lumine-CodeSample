using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class animator
{
    public static bool CheckAnimationTag(this Animator animator, string tagName, int animationLayay)//定义拓展方法
    {
        return animator.GetCurrentAnimatorStateInfo(animationLayay).IsTag(tagName);//判断当前播放的动画是否包含某个标签
    }
    //废弃方法
    //public static void SetFloatLerp(this Animator animator,float horizontal,float vertical)
    //{
    //    float smoothTime = 0.1f;
    //    animator.SetFloat("vertical", Mathf.Lerp(animator.GetFloat("vertical"), vertical,smoothTime*Time.deltaTime));
    //    animator.SetFloat("horizontal", Mathf.Lerp(animator.GetFloat("horizontal"), horizontal, smoothTime * Time.deltaTime));
    //}
}
