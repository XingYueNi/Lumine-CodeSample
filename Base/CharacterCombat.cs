using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Enemy;

public class CharacterCombat : MonoBehaviour
{
    [Header("战斗数值")]
    [Tooltip("拖一个 CombatStats；留空会自动从 Resources/Combat/<statsAsset> 加载")]
    public CombatStats stats;

    [Tooltip("上面留空时，从 Resources/Combat/ 下按这个名字找。玩家用 PlayerStats，敌人用 EnemyStats")]
    public string statsAsset = "";

    [Tooltip("兜底攻击数据：子类没覆盖时使用")]
    public AttackData defaultAttack;

    [Tooltip("defaultAttack 留空时，从 Resources/Combat/ 下按这个名字找")]
    public string defaultAttackAsset = "EnemyAttack1";

    /// <summary>格挡时的伤害倍率（由 stats 覆盖，这里给默认值保底）</summary>
    protected float blockDamageMultiplier = 0.1f;
    /// <summary>格挡时的架势倍率</summary>
    protected float blockPostureMultiplier = 0.5f;

    private bool statsLoaded;
    private bool defaultAttackLoaded;

    /// <summary>当前生效的属性：优先 Inspector 里拖的，其次 Resources，最后代码默认值</summary>
    protected CombatStats Stats
    {
        get
        {
            if (stats == null && !statsLoaded)
            {
                statsLoaded = true;
                stats = CombatConfig.LoadStats(statsAsset);
            }
            return stats;
        }
    }

    protected AttackData DefaultAttack
    {
        get
        {
            if (defaultAttack == null && !defaultAttackLoaded)
            {
                defaultAttackLoaded = true;
                defaultAttack = CombatConfig.LoadAttack(defaultAttackAsset);
            }
            return defaultAttack;
        }
    }

    private float maxHealth = 100;
    private float maxPosture = 100;
    private float postureRecoverySpeed = 10;//恢复速度
    private float postureRecoveryDelay = 1.5f;//受到攻击之后延迟几秒才恢复架势条
    private float currentHealth;
    public float CurrentHealth
    {
        get
        {
            return currentHealth;
        }
        set
        {
            currentHealth = value;
            OnHealthChanged?.Invoke(currentHealth);
        }
    }
    public float currentPosture;
    protected float lastPostureDamageTime;//最后一次受击，用来判定什么时候回复架势条
    protected bool isDefending;

    protected bool isAttacking;
    protected bool isParryWindowOpen;
    protected float defendStartTime;
    protected float parryWindowDuration = 0.5f;//完美弹反时间窗口
    protected bool isAttackWindowOpen;
    public bool isCanControl=true;
    public bool isDeath=false;
    public event Action<float> OnHealthChanged;
    public event Action<float> OnPostureChanged;
    public event Action OnStagger;
    public event Action OnDeath;
    public bool invincibility;//是否无敌
    [Header("处决基础")]
    protected bool isExecuted;
    public bool isStaggered;
    public float staggerTimer;              // 破防剩余时间
    public float staggerDuration = 4f;      // 破防持续时间
    public bool isBeingExecuted;            // 是否正在被处决
    public bool isExecuting;                // 是否正在执行处决

    public bool CanBeExecuted => isStaggered && staggerTimer > 0f && !isDeath && !isBeingExecuted;

    protected virtual void Start()
    {
        ApplyStats();
        currentHealth = maxHealth;
        currentPosture = 0;
    }

    /// <summary>把 SO 里的数值灌进运行时字段。没配 SO 就保持代码默认值。</summary>
    protected virtual void ApplyStats()
    {
        CombatStats s = Stats;
        if (s == null) return;

        maxHealth = s.maxHealth;
        maxPosture = s.maxPosture;
        postureRecoverySpeed = s.postureRecoverySpeed;
        postureRecoveryDelay = s.postureRecoveryDelay;
        blockDamageMultiplier = s.blockDamageMultiplier;
        blockPostureMultiplier = s.blockPostureMultiplier;
        parryWindowDuration = s.parryWindow;
        staggerDuration = s.staggerDuration;
    }
    public void StartDefend()//开始防御
    {
        isDefending = true;
        defendStartTime = Time.time;
        isParryWindowOpen = true;
        // 触发防御动画
    }

    public void StopDefend()
    {
        isDefending = false;
        isParryWindowOpen = false;
    }

    public bool IsInParryWindow()
    {
        return isDefending && (Time.time - defendStartTime) <= parryWindowDuration;
    }

    public virtual void TakeDamage(float damage, float postureDamage)//受到伤害，弹反逻辑在对方的攻击方法内处理，故此处只处理普通防御和未防御
    {
        if (isDefending)//是否正在防御
        {
            // 普通防御：减少伤害，增加架势
            CurrentHealth -= damage * blockDamageMultiplier;
            AddPosture(postureDamage * blockPostureMultiplier);
        }
        else
        {
            CurrentHealth -= damage;
            AddPosture(postureDamage);
            //播放受击动画
        }
        lastPostureDamageTime = Time.time;//更新受击时间
        if (CurrentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        isDeath = true;
        OnDeath?.Invoke();
        Debug.Log("死亡");
    }

    public virtual void  AddPosture(float amount)
    {
        currentPosture += amount;
        currentPosture = Mathf.Clamp(currentPosture, 0, maxPosture);
        OnPostureChanged?.Invoke(currentPosture);
        if (currentPosture >= maxPosture)
        {
            Debug.Log("破韧");
            Stagger();//破韧状态
        }
    }

    protected virtual void Stagger()
    {
        OnStagger?.Invoke();
        isStaggered = true;
        isExecuted = true;
        // 播放失衡动画
    }

    public void EnableAttackDetection()//在时间窗口内可以连续攻击
    {
        isAttackWindowOpen = true;
    }
    public void EnableCanControl()//当前可以操控，包括攻击前摇转防御
    {
        isCanControl = true;

    }
    public void EnableAttacking()//允许攻击
    {
        isAttacking = true;
    }
    public void DisableCanControl()//当前不可以操控，各种动作后摇，以及受击破韧之类
    {
        isCanControl = false;
    }

    public void DisableAttackDetection()
    {
        isAttackWindowOpen = false;
    }
    
    public void DisableAttacking()//正在攻击
    {
        isAttacking = false;
    }

    protected virtual void Update()
    { // 架势条恢复逻辑
        if (Time.time - lastPostureDamageTime > postureRecoveryDelay&&(isDefending || !isAttacking) )//当前在防御，或者未攻击并且上次被攻击之后经过几秒的延迟后开始回复
        {
            AddPosture( -(postureRecoverySpeed * Time.deltaTime));
            
        }
    }
    public virtual void OnAttackHit(PlayerManager target)//判定对方是否在弹反时间窗口内
    {
        AttackData atk = DefaultAttack;
        float selfPosture = atk != null ? atk.parrySelfPosture : 10f;
        float dmg = atk != null ? atk.damage : 10f;
        float posture = atk != null ? atk.postureDamage : 15f;

        if (target.IsInParryWindow())//如果对方正在弹反
        {
            AddPosture(selfPosture);//自身架势条上涨，玩家脚本重写方法，自身架势条不上涨
        }
        target.TakeDamage(dmg, posture);
        
    }
    public void disableInvincibility()
    {
        invincibility = false;
     //   Debug.Log("无敌时间结束");
    }
}

