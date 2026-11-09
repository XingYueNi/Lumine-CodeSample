using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class KnifeManager : MonoBehaviour
{
    public GameObject target;
    public GameObject reset;
    public Animator animator;
    private void Awake()
    {
        GameObject playerGo = GameObject.Find("Player");
        if (playerGo != null) animator = playerGo.GetComponent<Animator>();
        if (reset != null) reset.SetActive(false);
    }
    private void Update()
    {
        if (target.activeSelf && animator.CheckAnimationTag("Run", 0))//如果武器在手上，而现在需要收起
        {
            NoKnife();
        }
        else
        if(!target.activeSelf&& !animator.CheckAnimationTag("Run", 0))
        {
            Knife();
        }
            
    }
    public void Knife()
    {
        reset.SetActive(false);
        target.SetActive(true);
    }
    public void NoKnife()
    {
        reset.SetActive(true);
        target.SetActive(false);
    }


}

