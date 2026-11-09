using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Audio:MonoBehaviour
{
    public AudioSource attack;//攻击音效
    public AudioSource hit;//受击音效（包含弹反）
    public AudioSource voice;//角色语音
    public AudioSource bck;//(背景音乐)
    public AudioSource foot;
    public AudioClip[] attackClips;
    public AudioClip[] parryClips;
    public AudioClip[] guardClips;
    public AudioClip[] hitClips;
    public Animator animator;
    public void Sound()
    {
        if (animator.CheckAnimationTag("Attack", 0)||((animator.CheckAnimationTag("combo_01", 0) || animator.CheckAnimationTag("combo_02", 0) || animator.CheckAnimationTag("combo_03", 0))))//动画为攻击动画
        {
            AttackSound();
        }
        else
        if(animator.CheckAnimationTag("Hit", 0))//动画为受击动画
        {
            HitSound();
        }
        else
        if(animator.CheckAnimationTag("Gurad", 0))//动画为格挡动画
        {
            GuradSound();
        }
        else
        if(animator.CheckAnimationTag("Parry", 0))//动画为弹反动画
        {
            ParrySound();
        }
    }

    public void HitSound()
    {
        hit.clip = hitClips[Random.Range(0, hitClips.Length)];
        hit.PlayOneShot(hit.clip);
    }
    public void AttackSound()
    {
        attack.clip = attackClips[Random.Range(0, attackClips.Length)];
        attack.Play();
    }
    public void ParrySound()
    {
        hit.clip = parryClips[Random.Range(0, parryClips.Length)];
        hit.Play();
    }
    public void GuradSound()
    {
        hit.clip = guardClips[Random.Range(0, guardClips.Length)];
        hit.Play();
    }
}
