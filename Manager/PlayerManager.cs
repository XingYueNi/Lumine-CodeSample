using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : CharacterCombat
{

    private static PlayerManager player;
    private Animator animator;
    public event Action OnAttackStarted;
    private bool isTakeDamage;
    private bool isAttackWindow;
    [Header("射线攻击判定")]
    public Transform[] raycastPoints;      // 武器上的射线点

    private SwordTrail swordTrail;//刀光拖尾（组件没挂就不播）
    private SwordTrail Trail
    {
        get
        {
            if (swordTrail == null) swordTrail = GetComponent<SwordTrail>();
            return swordTrail;
        }
    }
    [Header("弹反火花生成点（留空则自动取 raycastPoints 里离角色最远的点）")]
    public Transform parryPoint;

    [Header("弹反方向判定")]
    [Tooltip("以玩家本地坐标系的这个高度作为「上段/下段」的分界（米，0=脚底）。大约取胸口高度。")]
    public float parryHeightPivot = 1.1f;

    [Tooltip("左右判定的死区（米）。正面命中时左右会抖，小于这个值就沿用上一次的方向。")]
    public float parrySideDeadzone = 0.05f;

    private bool lastParryRight = true;

    [Header("攻击数值")]
    [Tooltip("普攻命中时用这份；留空则从 Resources/Combat/PlayerAttack1 加载")]
    public AttackData lightAttack;
    public string lightAttackAsset = "PlayerAttack1";

    [Tooltip("第二条攻击判定用这份；留空则从 Resources/Combat/PlayerAttack2 加载")]
    public AttackData heavyAttack;
    public string heavyAttackAsset = "PlayerAttack2";

    private bool lightAttackLoaded;
    private bool heavyAttackLoaded;

    public AttackData LightAttack
    {
        get
        {
            if (lightAttack == null && !lightAttackLoaded)
            {
                lightAttackLoaded = true;
                lightAttack = CombatConfig.LoadAttack(lightAttackAsset);
            }
            return lightAttack;
        }
    }

    public AttackData HeavyAttack
    {
        get
        {
            if (heavyAttack == null && !heavyAttackLoaded)
            {
                heavyAttackLoaded = true;
                heavyAttack = CombatConfig.LoadAttack(heavyAttackAsset);
            }
            return heavyAttack;
        }
    }
    private Transform bladeTipCache;//弹反火花生成点：刀身最末端
    public Vector3 BladeTipPosition()
    {
        if (parryPoint != null) return parryPoint.position;
        if (bladeTipCache == null && raycastPoints != null)
        {
            float best = -1f;
            for (int i = 0; i < raycastPoints.Length; i++)
            {
                Transform p = raycastPoints[i];
                if (p == null) continue;
                float d = Vector3.Distance(p.position, transform.position);
                if (d > best) { best = d; bladeTipCache = p; }
            }
        }
        return bladeTipCache != null
            ? bladeTipCache.position
            : transform.position + transform.forward * 0.8f;
    }
    public LayerMask targetLayer;          // 目标层（敌人层）
    public float rayRadius = 0.1f;         // 射线半径

    private bool isHitboxActive = false;
    private Dictionary<Transform, Vector3> lastPositions = new Dictionary<Transform, Vector3>();
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    private CharacterController cc;
    public GameObject ExecutionEnemy;//敌人在自身脚本中判断是否能被处决，当玩家进入处决指定范围时，由敌人脚本传递给玩家
    private void Awake()
    {
        player = this;              // 玩家自己注册，省掉每次 Find
        if (string.IsNullOrEmpty(statsAsset)) statsAsset = "PlayerStats";
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
    }
    public static PlayerManager Player
    {
        get
        {
            if (player == null)
            {
                // 主菜单场景（Level_0）里本来就没有 Player，这里必须允许返回 null。
                // 原写法是 GameObject.Find("Player").GetComponent<...>()，
                // 找不到时 Find 返回 null，直接 GetComponent 就会空引用。
                GameObject go = GameObject.Find("Player");
                if (go != null) player = go.GetComponent<PlayerManager>();
            }
            return player;
        }
    }
    protected override void Update()
    {
        base.Update();
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        if (Input.GetMouseButtonDown(1))//格挡
        {
            if (isCanControl)//玩家当前状态是否允许操作，用动画事件控制
            {
                StartDefend();
                animator.SetBool("isGuard", true);
                animator.ResetTrigger("latt");//重置连击判定，防止多判
                                              // Debug.Log("开始格挡");
            }

        }
        else
        if (Input.GetMouseButtonUp(1))
        {
            StopDefend();
            animator.ResetTrigger("PerfectParry");
            animator.SetBool("isGuard", false);
            animator.SetBool("guardToIdle", false);//允许玩家在取消格挡的一定时间窗口内重新格挡，由动画事件控制
            // Debug.Log("格挡结束");
        }
        if (isHitboxActive && raycastPoints != null)
        {
            foreach (var point in raycastPoints)
            {
                if (point == null) continue;
                if (!lastPositions.ContainsKey(point)) continue;

                Vector3 currentPos = point.position;
                Vector3 lastPos = lastPositions[point];

                DetectSweep(lastPos, currentPos);
                lastPositions[point] = currentPos;
            }
        }
    }

    void DetectSweep(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;

        // 可视化调试：保留 1 秒的轨迹线
        Debug.DrawLine(from, to, Color.green, 1f);

        if (distance < 0.001f) return;

        RaycastHit[] hits = Physics.SphereCastAll(from, rayRadius, direction.normalized, distance, targetLayer);

        foreach (var hit in hits)
        {
            GameObject root = hit.transform.root.gameObject;
            if (root == gameObject) continue;
            if (hitTargets.Contains(root)) continue;
            hitTargets.Add(root);
            /*
            GameObject debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugCube.transform.position = hit.point;
            debugCube.transform.localScale = Vector3.one * 0.1f; // 小方块
            debugCube.GetComponent<Renderer>().material.color = Color.green;

            // 移除碰撞体，避免影响后续物理检测
            Destroy(debugCube.GetComponent<Collider>());

            // 2 秒后自动销毁
            Destroy(debugCube, 2f);
            */
            Enemy target = hit.transform.GetComponentInParent<Enemy>();
            if (target != null)
            {
                OnAttack(target);
            }
        }
    }
    public void EnableHitbox()
    {
        isHitboxActive = true;
        if (Trail != null) Trail.Begin();
        hitTargets.Clear();
        lastPositions.Clear();

        foreach (var point in raycastPoints)
        {
            if (point != null)
                lastPositions[point] = point.position;
        }
    }

    // 动画事件：攻击判定结束
    public void DisableHitbox()
    {
        isHitboxActive = false;
        if (Trail != null) Trail.End();
    }


    void OnDrawGizmos()
    {
        if (!isHitboxActive || raycastPoints == null) return;
        Gizmos.color = Color.blue;
        foreach (var point in raycastPoints)
        {
            if (point == null) continue;
            if (lastPositions.ContainsKey(point))
            {
                Gizmos.DrawLine(lastPositions[point], point.position);
            }
        }
    }
    public void AttackEnable()
    {
        animator.SetBool("Attack_1", false);
    }
    public void StartAttack()
    {
        OnAttackStarted?.Invoke();
    }
    
    public void Executed()//被处决
    {
        animator.Play("Executed", 0, 0);
        CurrentHealth -= 100;
    }
    public void Execution()//处决
    {
        if (ExecutionEnemy.GetComponent<Enemy>().Executed() == 1)//正面处决
            animator.Play("Execution");
        else
            animator.Play("Deathblow");
    }
    public void Attack()
    {
        if (ExecutionEnemy != null)//玩家当前可以处决敌人
        {
            Execution();
            return;
        }
        if (isCanControl || isAttackWindowOpen)//当前是否能够操控
        {
            animator.SetTrigger("latt");
        }

    }
    public void OnAttackHit(Enemy target)
    {
        // 目标已经被处决 / 已死，不再走弹反判定与特效
        if (target == null || target.isDeath) return;

        if (target.IsInParryWindow())//如果对方正在弹反
        {
            //播放弹开动画
            animator.Play("Parry");
            ParryFlash.Spawn(BladeTipPosition(), Vector3.zero, 1.1f);
        }
        else
        {
            AttackData atk = LightAttack;
            target.TakeDamage(atk != null ? atk.damage : 5f,
                              atk != null ? atk.postureDamage : 35f);
        }
    }
    public void TakeDamage(float damage, float postureDamage, Transform hitDirection)
    {
        if (isDeath) return;
        lastPostureDamageTime = Time.time;//更新受击时间
        // 击退方向 = 远离攻击源。原写法减去的是玩家自己，恒等于 0 向量，
        // 所以玩家受击从来没有被击退过。
        Vector3 attackerPos = hitDirection != null ? hitDirection.position : transform.position + transform.forward;
        Vector3 knockbackDir = transform.position - attackerPos;
        knockbackDir.y = 0;
        if (knockbackDir.sqrMagnitude > 0.001f)
        {
            StartCoroutine(KnockbackCoroutine(knockbackDir, 1f, 0.25f));
        }
        

        if (IsInParryWindow())
        {
            HitDirection(hitDirection);//计算玩家受击方向并播放对应弹反动画
                                       //   Debug.Log("玩家弹反");
            return;
        }
        if (isDefending)//是否正在防御
        {
            // 普通防御：减少伤害，增加架势
            CurrentHealth -= damage * 0.1f; // 减免90%
            AddPosture(postureDamage * 2);
            animator.Play("GuardHit", 0, 0);
        }
        else
        {
            CurrentHealth -= damage;
            AddPosture(postureDamage);
            animator.Play("Hit", 0, 0); //第三个参数 0f 表示从动画开头（0 %）开始播放。
                                        //  Debug.Log("播放受击动画");
        }
        if (CurrentHealth <= 0)
            Die();

    }
    private void HitDirection(Transform hitDirection)
    {
        // 攻击点：优先用传入的武器/命中点；没有就退化成玩家正前方，避免空引用
        Vector3 hitWorld = hitDirection != null
            ? hitDirection.position
            : transform.position + transform.forward * 1.2f;

        // 转换到玩家本地坐标：x>0 在右，y 是相对脚底的高度
        Vector3 localHit = transform.InverseTransformPoint(hitWorld);

        // 左右：本地 x 的符号；正面命中时 x 接近 0，加死区避免左右乱跳
        bool isRight;
        if (Mathf.Abs(localHit.x) < parrySideDeadzone)
        {
            isRight = lastParryRight;
        }
        else
        {
            isRight = localHit.x > 0f;
            lastParryRight = isRight;
        }

        // 上下：关键修复。原实现是 localHit.y > 0，而武器高度永远高于脚底，
        // 所以 isUp 恒为 true，两条 Down 分支从来进不去。改成和胸口高度比较。
        bool isUp = localHit.y > parryHeightPivot;
        if (isRight && isUp) animator.Play("Parry_RightUp", 0, 0f);
        else if (isRight && !isUp) animator.Play("Parry_RightDown", 0, 0f);
        else if (!isRight && isUp) animator.Play("Parry_LeftUp", 0, 0f);
        else animator.Play("Parry_LeftDown", 0, 0f);
        // 所有弹反火花统一生成在玩家刀尖这一个点上，保证不会分散
        Vector3 clashPos = BladeTipPosition();
        Vector3 hitPos = hitDirection != null ? hitDirection.position : clashPos + transform.forward;
        ParryFlash.Spawn(clashPos, (hitPos - clashPos).normalized, 1f);
        StartCoroutine(GameManager.TimeScale());
    }
    protected override void Stagger()
    {
        base.Stagger();
        animator.SetBool("isBreak", true);
    }
    protected void RecStagger()
    {
        AddPosture(-100);
        isStaggered = false;
        animator.SetBool("isBreak", false);
    }

    public void GuardToIdleTure()//允许切到待机
    {
        animator.SetBool("guardToIdle", true);
    }
    protected override void Die()
    {
        base.Die();
        isDeath = true;
        animator.SetBool("isDeath", true);
    }
    public void OnAttack(Enemy target)
    {
        if (!isAttackWindow) return;//当前不在可造成伤害的时间窗口内
        if (isTakeDamage) return;
        isTakeDamage = true;//已经造成过伤害
                            //   Debug.Log("命中敌人");

        //   Debug.Log("对敌人造成伤害"+target.gameObject.name);
        AttackData atk = HeavyAttack;
        target.TakeDamage(atk != null ? atk.damage : 5f,
                          atk != null ? atk.postureDamage : 50f);
    }
    public void SetHealth(float newHP)
    {
        CurrentHealth = newHP;
    } 
    public void SetPosture(float newSP)
    {
        currentPosture = newSP;
    }
    public void PlayerDeath()
    {
     //   GameManager.Instance.PLayerDeath();
    }
    public void OnEnableTakeDamage()//允许对敌人造成伤害
    {
        isTakeDamage = false;
    }
    public void OnDisableDamage()
    {
        isTakeDamage = true;
    }
    public void AtAttackWindow()
    {
        isAttackWindow = true;//目前在动画事件的攻击窗口内
        EnableHitbox();
    }
    public void NoAttackWindow()
    {
        isAttackWindow = false;//目前不在动画事件的攻击窗口内
        DisableHitbox();
    }
    private IEnumerator KnockbackCoroutine(Vector3 direction, float distance, float duration)
    {
        if (cc == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + direction.normalized * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            Vector3 moveDelta = currentPos - transform.position;
            moveDelta.y = 0; // 保持水平后退，不影响重力

            cc.Move(moveDelta);
            yield return null;
        }
    }
}