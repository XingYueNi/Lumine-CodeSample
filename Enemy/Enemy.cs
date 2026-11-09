using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : CharacterCombat
{
    private Transform player;//玩家位置
    private Animator animator;

    private SwordTrail swordTrail;//刀光拖尾
    private SwordTrail Trail
    {
        get
        {
            if (swordTrail == null) swordTrail = GetComponent<SwordTrail>();
            return swordTrail;
        }
    }
    private float attackRange = 3.5f;//攻击距离
    private float confrontationDistance = 6f;//对峙距离
    private float playerDistance = 20f;//追击玩家距离
    private float minDistance = 1.5f;//距离玩家最小的距离
    public float distance;
    public bool isAttack;
    private float attackTimer;
    public string bossAction;//显示敌人目前状态，用于调试
    public bool isAbsoluteTerritory;//玩家处于敌人交战区域，敌人允许发动攻击
    private float confrontationTimer;//对峙时随机移动时间
    private bool isConfrontation;//是否对峙区域
    private int confrontationNum;//随机移动次数
    [SerializeField]
    private bool isAttackWindow = false;//防止敌人在非攻击状态对玩家造成伤害
    [SerializeField]
    private bool isTakeDamage;//是否已经造成过伤害,防止多次判定，每次攻击前调用动画事件重置条件

    [Header("攻击数值")]
    [Tooltip("按攻击序号（Attack_1/2/3）对应，留空则从 Resources/Combat/EnemyAttackN 加载")]
    public AttackData[] attacks;
    public string[] attackAssets = { "EnemyAttack1", "EnemyAttack2", "EnemyAttack3" };

    private int currentAttackIndex = 1;
    private readonly System.Collections.Generic.Dictionary<int, AttackData> attackCache =
        new System.Collections.Generic.Dictionary<int, AttackData>();

    /// <summary>按当前攻击序号取数值；没配就用 EnemyAttackN，再没有返回 null（调用方兜底）</summary>
    public AttackData GetAttackData(int attackIndex)
    {
        if (attacks != null && attackIndex >= 1 && attackIndex <= attacks.Length && attacks[attackIndex - 1] != null)
            return attacks[attackIndex - 1];

        if (attackCache.TryGetValue(attackIndex, out AttackData cached)) return cached;

        string asset = (attackAssets != null && attackIndex >= 1 && attackIndex <= attackAssets.Length)
            ? attackAssets[attackIndex - 1]
            : "EnemyAttack" + attackIndex;

        AttackData loaded = CombatConfig.LoadAttack(asset);
        attackCache[attackIndex] = loaded;
        return loaded;
    }
    public delegate bool IsStoping(bool var);
    public event IsStoping OnIsStoping;
    private bool Stoping;
    private bool isIdle = false;
    public bool isStoping//敌人移动是否停止
    {
        set
        {
            OnIsStoping?.Invoke(Stoping);
        }
    }
    public delegate bool IsDeath(bool var);
    //  public static event IsDeath OnDead;
    public bool isWaitExecution = false;
    private static Enemy enemy;
    [Header("射线点")]
    public Transform[] raycastPoints;      // 武器上的射线点
    public LayerMask targetLayer;          // 目标层
    public float rayRadius = 0.1f;        // 射线半径（用 SphereCast 时生效）

    private bool isHitboxActive = false;
    private Dictionary<Transform, Vector3> lastPositions = new Dictionary<Transform, Vector3>();
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    private CharacterController cc;
    public GameObject Canvans;
    [Header("处决提示")]
    public GameObject executionPrompt;
    // public GameObject executionTxt;
    private bool canBeExecuted = false;     // 当前是否可被处决
    [Header("视野")]
    public float viewDistance = 10f;        // 视野距离
    public float detectAngle = 120;          // 视野角度（总角度，比如120度表示±60度）
    public LayerMask obstacleLayer;         // 遮挡层（墙壁、地形等）
    public float alertAngle = 280f;         // 警戒区总角度
    public LayerMask playerLayer;
    [Header("警戒值")]
    public float alertValue = 0f;
    public float maxAlert = 100f;
    public float alertIncreaseSpeed = 300f;    // 警戒区每秒累积速度
    public float alertDecaySpeed = 20f;       // 失去视野后衰减速度
    public float alertDecayDelay = 1.5f;      // 失去视野后多久开始衰减
    public float lastSeenTime;


    [SerializeField] private bool canSeePlayer = false;  // 是否看到玩家,如果看见玩家则自动索敌
    public float losePlayerTime = 2f;                    // 失去视野后多久放弃追击

    public static Enemy Instance
    {
        get
        {
            if (enemy == null) Debug.LogError("实例未找到");
            return enemy;
        }
        set
        {
            enemy = value;
        }
    }
    private void Awake()
    {
        Instance = this;
        if (string.IsNullOrEmpty(statsAsset)) statsAsset = "EnemyStats";
        GameObject playerGo = GameObject.Find("Player");
        if (playerGo != null) player = playerGo.transform;
        else Debug.LogWarning("[Enemy] 场景里找不到名为 Player 的物体，敌人 AI 无法工作");
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        executionPrompt.SetActive(false);
        //  executionTxt.SetActive(false);
    }
    protected override void Start()
    {
        base.Start();
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RegisterEnemy(gameObject.transform);
    }

    protected override void Update()
    {
        if (isDeath) return;
        distance = Vector3.Distance(transform.position, player.position);//持续判定与玩家的距离

        UpdateAlert();
        // 根据是否看到玩家切换行为
        if (canSeePlayer)
        {
            Canvans.gameObject.SetActive(true);
            Rotation();
            Move();
        }
        else
        {
            Canvans.gameObject.SetActive(false);
            Idle();
            isAbsoluteTerritory = false;
        }
        if (alertValue == 0)//如果玩家不在敌人警戒范围内的话，允许玩家处决敌人
        {
            WaitExecution(true);
        }
        else
        {
            WaitExecution(false);
        }
        if (isStaggered)
        {
            if (canBeExecuted) WaitExecution(true);
            staggerTimer -= Time.deltaTime;//失衡状态持续四秒
            if (staggerTimer <= 0f) RecStagger();
            return;
        }
        base.Update();
        if (PlayerManager.Player.isDeath)
        {
            //   Debug.Log("玩家死亡，放弃攻击");
            if (!isIdle)
                Idle();//如果玩家死亡，放弃攻击
            return;
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= 7f && isAbsoluteTerritory)//十秒执行一次攻击
        {
            Attack();
            attackTimer = 0;
        }
        if (!isHitboxActive) return;//射线攻击检测判定
        foreach (var point in raycastPoints)
        {
            if (point == null) continue;

            Vector3 currentPos = point.position;
            Vector3 lastPos = lastPositions[point];

            // 从上一帧位置到当前帧位置做射线检测
            DetectSweep(lastPos, currentPos);

            // 更新记录
            lastPositions[point] = currentPos;
        }
    }
    private void OnEnable()
    {
        OnIsStoping += RunState;
        PlayerManager.Player.OnAttackStarted += EnemyAction;
    }
    void OnDrawGizmosSelected()
    {
        // 发现区（120°）
        Gizmos.color = canSeePlayer ? Color.red : Color.green;
        DrawVisionArc(detectAngle, viewDistance, 12);

        // 警戒区（180°）
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
        DrawVisionArc(alertAngle, viewDistance, 16);
    }
    void DrawVisionArc(float totalAngle, float radius, int segments = 12)
    {
        float half = totalAngle * 0.5f;
        //  Vector3 origin = transform.position + Vector3.up * 1.5f; // 眼睛高度
        Vector3 origin = transform.position;

        // 两条边线
        Vector3 leftDir = Quaternion.Euler(0, -half, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, half, 0) * transform.forward;
        Gizmos.DrawLine(origin, origin + leftDir * radius);
        Gizmos.DrawLine(origin, origin + rightDir * radius);

        // 弧线：只在边缘点之间连线
        Vector3 prevPoint = origin + leftDir * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -half + (totalAngle / segments) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 currentPoint = origin + dir * radius;

            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
    public override void TakeDamage(float damage, float postureDamage)//受到伤害，弹反逻辑在对方的攻击方法内处理，故此处只处理普通防御和未防御
    {
        if (invincibility) return;
        if (isDeath) return;
        if (isStaggered) return;
        if (!canSeePlayer) canSeePlayer = true;
        lastPostureDamageTime = Time.time;//更新受击时间
        Vector3 knockbackDir = transform.position - PlayerManager.Player.transform.position;
        knockbackDir.y = 0;
        if (knockbackDir.sqrMagnitude > 0.001f)
        {
            StartCoroutine(KnockbackCoroutine(knockbackDir, 1f, 0.25f));
        }

        if (Random.value > 0.7f && IsInParryWindow())
        {
            Parry();
            return;
        }
        if (isDefending)//是否正在防御
        {
            // 普通防御：减少伤害，增加架势
            CurrentHealth -= damage * 0.1f; // 减免90%
            animator.Play("GuradHit", 0, 0);
            AddPosture(postureDamage * 2);
        }
        else
        {
            CurrentHealth -= damage;
            animator.Play("Hit", 0, 0);
            AddPosture(postureDamage);
            //Debug.Log("敌人播放受击动画");
            //播放受击动画
        }
        if (CurrentHealth <= 0)
            Die();
    }
    protected override void Die()
    {
        isDeath = true;
        animator.Play("Death");
        Destroy(gameObject, 3f);
        //Debug.Log("死亡");
    }
    protected override void Stagger()
    {
        base.Stagger();
        animator.SetBool("IsAttack", false);
        animator.Play("Break");
        StartCoroutine(GameManager.ExecutionSlowMotion());//失衡慢动作
      //  Debug.Log("播放破韧动画");
        canBeExecuted = true;
        staggerTimer = staggerDuration;
    }
    public void WaitExecution(bool value)
    {
        if (executionPrompt == null || PlayerManager.Player == null) return;

        bool inRange = value && distance <= 2f;
        bool owns = inRange && TryClaimExecutionTarget();

        // 只切换自己的提示图标；SetActive 相同值不会触发 OnEnable/OnDisable，安全
        if (executionPrompt.activeSelf != owns) executionPrompt.SetActive(owns);

        // 关键：只有「玩家当前登记的处决目标就是我」时才清空。
        // 原来无条件写 null，场上每个敌人每帧都在调这个方法，
        // 于是 A 刚登记上，B 下一帧就把它清掉了 —— 走过去永远处决不了。
        if (!owns && PlayerManager.Player.ExecutionEnemy == gameObject)
        {
            PlayerManager.Player.ExecutionEnemy = null;
        }
    }

    /// <summary>
    /// 抢处决目标：没被占就占，被自己占就保持，被别人占就比较距离，近的赢。
    /// 这样多个敌人同时在范围内时，玩家处决的是离得最近的那只，而不是看 Awake 顺序。
    /// </summary>
    private bool TryClaimExecutionTarget()
    {
        if (PlayerManager.Player == null) return false;

        GameObject current = PlayerManager.Player.ExecutionEnemy;
        if (current == null || current == gameObject)
        {
            PlayerManager.Player.ExecutionEnemy = gameObject;
            return true;
        }

        Vector3 playerPos = PlayerManager.Player.transform.position;
        float mineSqr = (playerPos - transform.position).sqrMagnitude;
        float theirsSqr = (current.transform.position - playerPos).sqrMagnitude;

        if (mineSqr < theirsSqr)
        {
            PlayerManager.Player.ExecutionEnemy = gameObject;
            return true;
        }
        return false;
    }
    public int Executed()//被处决
    {
        if (alertValue <= 0)//背后处决
        {
            ResetToFrontOfPlayer();
            animator.Play("Deathblowed", 0, 0f);
        }
        else
        {

            ResetTransform();
            animator.Play("Executed", 0, 0.1f);
        }

        executionPrompt.SetActive(false);
        CurrentHealth -= 100;
        PlayerManager.Player.ExecutionEnemy = null;
        isDeath = true;
        Destroy(gameObject, 4f);
       // Debug.Log("被处决死亡");
        if (alertValue > 0) return 1;
        else return 0;
    }
    private void ResetTransform()
    {
        Vector3 playerForward = player.forward;
        playerForward.y = 0;
        if (playerForward.sqrMagnitude < 0.001f) return;
        playerForward.Normalize();

        Vector3 targetPos = player.position + playerForward * 1f;
        targetPos.y = transform.position.y;

        if (cc != null) cc.enabled = false;
        transform.position = targetPos;
        if (cc != null) cc.enabled = true;

        Vector3 lookDir = player.position - targetPos;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
    }
    public void ResetToFrontOfPlayer(float distance = 0.38f)
    {
        Vector3 playerForward = player.forward;
        playerForward.y = 0;
        playerForward.Normalize();

        Vector3 targetPos = player.position + playerForward * distance;
        targetPos.y = transform.position.y;

        if (cc != null) cc.enabled = false;
        transform.position = targetPos;
        if (cc != null) cc.enabled = true;
        gameObject.transform.forward = player.transform.forward;
    }
    public void RecStagger()
    {
        isStaggered = false;
        canBeExecuted = false;
        if (executionPrompt != null)
        {
            //    executionTxt.SetActive(false);
            executionPrompt.SetActive(false);
        }


        // 恢复到可战斗状态
        animator.Play("Idle");
        AddPosture(-100); // 架势条清零
    }
    private void Idle()
    {
        animator.SetBool("isIdle", true);
        animator.SetBool("isMove", false);
        animator.SetBool("IsAttack", false);
        isIdle = true;
    }

    private int CheckVisionLevel()  // 返回：0 = 什么都看不到；1 = 在警戒区（累积）；2 = 在发现区（立即发现）
    {
        if (PlayerManager.Player == null || PlayerManager.Player.isDeath)
            return 0;

        Vector3 toPlayer = PlayerManager.Player.transform.position - transform.position;
        if (toPlayer.sqrMagnitude > viewDistance * viewDistance)
            return 0;

        Vector3 flatDir = toPlayer;
        flatDir.y = 0;
        if (flatDir.sqrMagnitude < 0.001f) return 0;

        float angle = Vector3.Angle(transform.forward, flatDir);
        if (angle > alertAngle * 0.5f)
            return 0;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 playerPos = PlayerManager.Player.transform.position + Vector3.up * 1f;
        if (Physics.Linecast(eyePos, playerPos, obstacleLayer))
            return 0;

        if (angle <= detectAngle * 0.5f)
            return 2;

        return 1;
    }

    private void UpdateAlert()
    {
        int visionLevel = CheckVisionLevel();

        if (visionLevel == 2)
        {
            // 发现区：立即发现
            alertValue = maxAlert;
            lastSeenTime = Time.time;
        }
        else if (visionLevel == 1)
        {
            // 警戒区
            if (distance <= 2.5f)
            {
                // 距离小于氛围：固定速度上涨
                alertValue += maxAlert * 0.9f * Time.deltaTime;
            }
            else
            {
                //距离 > 1m：按角度和距离加权累积 ======
                Vector3 toPlayer = PlayerManager.Player.transform.position - transform.position;
                Vector3 flatDir = toPlayer;
                flatDir.y = 0;
                float angle = Vector3.Angle(transform.forward, flatDir);

                float halfAlert = alertAngle * 0.5f;
                float halfDetect = detectAngle * 0.5f;
                float angleFactor = Mathf.Clamp01(Mathf.Lerp(1f, 0.3f, (angle - halfDetect) / (halfAlert - halfDetect)));

                float distanceRatio = Mathf.Clamp01(distance / viewDistance);
                float distanceFactor = Mathf.Lerp(1f, 0.1f, distanceRatio * distanceRatio);

                alertValue += alertIncreaseSpeed * angleFactor * distanceFactor * Time.deltaTime;
            }

            alertValue = Mathf.Min(alertValue, maxAlert);
            lastSeenTime = Time.time;
        }
        else
        {
            // 不在视野范围内：延迟后衰减，绝对不上升
            if (Time.time - lastSeenTime > alertDecayDelay)
            {
                alertValue = Mathf.Max(alertValue - alertDecaySpeed * Time.deltaTime, 0f);
            }
        }

        // 是否发现玩家
        bool nowAlerted = alertValue >= maxAlert;

        if (nowAlerted && !canSeePlayer)
        {
            canSeePlayer = true;
            OnAlerted();
        }
        else if (!nowAlerted && canSeePlayer && alertValue <= 0f)
        {
            canSeePlayer = false;
            OnLostPlayer();
        }
    }

    private void OnAlerted()
    {
       // Debug.Log("敌人发现玩家");
    }

    private void OnLostPlayer()
    {
       // Debug.Log("敌人失去目标");
    }


    public bool RunState(bool var)
    {
        if (var == false)
        {
            animator.SetBool("isMove", true);
            animator.SetBool("isIdle", false);

        }
        else
        {
            animator.SetBool("isMove", false);
            animator.SetBool("isIdle", true);

        }
        return var;
    }
    public void EnemyAction()
    {
        //  Debug.Log("玩家发起攻击，进行决策");
        if (isDeath || isStaggered) return;
        if (!isAttack)//当前不处于攻击状态
        {
            if (isAbsoluteTerritory)
            {
                if (Random.value > 0.6f)//0.4的概率格挡
                {
                    isDefending = true;
                    Guard();//播放格挡动画
                }
                else if (Random.value < 0.4f)//重新判断，0.4的概率闪避，否则不处理，
                {
                    Desh();
                }
            }
            else
            {
               // Debug.Log("玩家在交战范围外");
            }
        }
        else
            //  Debug.Log("当前正在攻击，无法响应");
            return;
    }
    public void Guard()
    {
        //先摆出格挡架势，再判断后续操作
        bossAction = "格挡";
        StartDefend();
        animator.CrossFade("defense_0", 0.2f);
        isDefending = true;
        //   Debug.Log("敌人格挡");
    }
    public void Desh()
    {
        bossAction = "闪避";
        invincibility = true;
        animator.Play("Desh");
        // Debug.Log("敌人闪避");
    }
    public void Parry()
    {
        // 已死 / 正在被处决的敌人不该再弹反
        if (isDeath) return;

        bossAction = "弹反";
        animator.Play("Parry_2");
        ParryFlash.Spawn(PlayerManager.Player != null ? PlayerManager.Player.BladeTipPosition() : transform.position + Vector3.up * 1.3f, -transform.forward, 1.1f);
        StartCoroutine(GameManager.TimeScale());//顿帧
    }
    private void Move()//移动与追击逻辑
    {
        if (distance >= playerDistance)//超出追加距离则待机
        {
            bossAction = "超出距离，待机中";
            isAbsoluteTerritory = false;
            isStoping = true;
            return;
        }
        else//在追击范围内则向玩家方向移动
        {
            if (distance <= minDistance)
            {
                bossAction = "距离玩家过近，离开范围";
                isStoping = false;
                animator.SetFloat("Right", 0);
                animator.SetFloat("Forward", -1);
                return;
            }
            if (distance >= confrontationDistance)
            {
                bossAction = "超出对峙范围，向玩家移动";
                isStoping = false;
                isAbsoluteTerritory = false;
                animator.SetFloat("Right", 0);
                animator.SetFloat("Forward", 1);
            }
            else//进入对峙范围
            {
                isStoping = true;
                if (isConfrontation)
                    ConfrontationMove();//随机方向移动
                if (distance <= attackRange)
                {
                    bossAction = "进入交战领域准备攻击";
                    isStoping = true;
                    isAbsoluteTerritory = true;//允许敌人发动攻击
                    isConfrontation = false;
                }
                if (isConfrontation == false && confrontationNum == 0)
                {
                    bossAction = "对峙";
                    isConfrontation = true;
                }
            }

        }
    }
    public void ConfrontationMove()
    {
        confrontationTimer += Time.deltaTime;
        if (confrontationTimer > 2f)//每一次移动2秒
        {
            float randomValue_1, randomValue_2;//随机方向移动
            if (Random.value > 0.5f)
                randomValue_1 = Random.Range(0.6f, 0.9f);
            else
                randomValue_1 = Random.Range(-0.9f, -0.6f);


            if (Random.value > 0.5f)
                randomValue_2 = Random.Range(-0.9f, -0.6f);
            else
                randomValue_2 = Random.Range(0.6f, 0.9f);
            StartCoroutine(MoveLerp("Forward", randomValue_1));
            StartCoroutine(MoveLerp("Right", randomValue_2));
            confrontationTimer = 0;
            confrontationNum++;
            if (confrontationNum > 2) { isConfrontation = false; confrontationNum = 0; isStoping = false; }//如果已经移动三次了，则取消对峙状态,并且向玩家移动
        }
    }
    private IEnumerator MoveLerp(string name, float target)
    {
        float last = animator.GetFloat(name);
        if (last > target)
            while (last > target)
            {
                last -= 0.01f;
                animator.SetFloat(name, last);
                yield return null;
            }
        else
        {
            while (last < target)
            {
                last += 0.01f;
                animator.SetFloat(name, last);
                yield return null;
            }
        }
        yield break;
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
    private void Rotation()
    {
        // 计算水平方向（忽略 Y 轴）
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // 直接设为 0，确保在 XZ 平面

        // 如果方向向量接近零（玩家与敌人几乎重叠），跳过旋转
        if (direction.sqrMagnitude < 0.01f) return;

        // 目标旋转：让 forward 指向水平方向，up 保持世界向上
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        // 平滑旋转（使用 Slerp 或 RotateTowards 更可控）
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    }
    private void OnDestroy()
    {
        // 从敌人列表里注销，否则锁定逻辑会遍历到这个已销毁的 Transform
        if (EnemyManager.Instance != null) EnemyManager.Instance.UnregisterEnemy(transform);

        // 如果玩家当前锁着 / 盯着我处决，一起清掉
        if (PlayerManager.Player != null)
        {
            if (PlayerManager.Player.ExecutionEnemy == gameObject)
                PlayerManager.Player.ExecutionEnemy = null;
        }
    }

    public void Attack()
    {
        isAttack = true;
        animator.SetBool("IsAttack", isAttack);
        animator.SetBool("isMove", false);
        int i = Random.Range(1, 4);//产生随机数
        currentAttackIndex = i;   // 记下这次是哪一招，命中时按它取伤害
        string attackTYpe = "Attack_" + i.ToString();
        animator.SetTrigger(attackTYpe);
    }
    // private int n=0, x=0;
    public void OnAttackHit(PlayerManager target, Transform weaponTransform)
    {
        if (!isAttackWindow) return;//当前不在可造成伤害的时间窗口内
                                    // Debug.Log("命中玩家" + (++n) + "次");
        if (isTakeDamage) return;
        isTakeDamage = true;//已经造成过伤害

        AttackData atk = GetAttackData(currentAttackIndex);

        if (target.IsInParryWindow())//如果对方正在弹反
        {
            AddPosture(atk != null ? atk.parrySelfPosture : 10f);//自身架势条上涨，玩家脚本重写方法，自身架势条不上涨
        }
        target.TakeDamage(atk != null ? atk.damage : 10f,
                          atk != null ? atk.postureDamage : 15f,
                          weaponTransform);
        // Debug.Log("造成" + (++x) + "次伤害");
    }
    public void DashToPlayer()
    {
        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        if (player == null || cc == null) yield break;

        // 计算目标位置：朝玩家方向的一半距离
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        float distance = direction.magnitude;

        if (distance < 0.8f) yield break;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + direction.normalized * (distance * 0.7f);


        // 突进持续时间（可调，通常 0.1~0.2 秒）
        float duration = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // 当前位置
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            // 计算这一帧需要移动的位移
            Vector3 moveDelta = currentPos - transform.position;
            moveDelta.y = 0;

            // 用 CharacterController 移动（可以带上重力）
            //  moveDelta.y = verticalVelocity * Time.deltaTime;
            cc.Move(moveDelta);

            yield return null;
        }
    }
    public void EnableHitbox()
    {
        isHitboxActive = true;
        if (Trail != null) Trail.Begin();
        hitTargets.Clear();
        lastPositions.Clear();

        // 记录初始位置
        foreach (var point in raycastPoints)
        {
            lastPositions[point] = point.position;
        }
    }
    public void DisableHitbox()
    {
        isHitboxActive = false;
        if (Trail != null) Trail.End();
    }
    void DetectSweep(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;
        Debug.DrawLine(from, to, Color.red, 1f);

        if (distance < 0.001f) return; // 几乎没有移动，跳过

        // 方式一：普通射线（无半径）
        //  RaycastHit[] hits = Physics.RaycastAll(from, direction.normalized, distance, targetLayer);

        // 方式二：球形扫掠（有半径，更宽容）
        RaycastHit[] hits = Physics.SphereCastAll(from, rayRadius, direction.normalized, distance, targetLayer);

        foreach (var hit in hits)
        {
            GameObject root = hit.transform.root.gameObject;
            if (root == gameObject) continue;
            if (hitTargets.Contains(root)) continue;
            hitTargets.Add(root);

         /*   // 在命中位置生成一个临时立方体
            GameObject debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugCube.transform.position = hit.point;
            debugCube.transform.localScale = Vector3.one * 0.1f; // 小方块
            debugCube.GetComponent<Renderer>().material.color = Color.red;

            // 移除碰撞体，避免影响后续物理检测
            Destroy(debugCube.GetComponent<Collider>());

            // 2 秒后自动销毁
            Destroy(debugCube, 2f);
         */
            PlayerManager target = hit.transform.GetComponentInParent<PlayerManager>();
            if (target != null)
            {
                OnAttackHit(target, hit.transform);
            }
        }
    }
    public void AtAttackWindow()
    {
        isAttackWindow = true;//目前在动画事件的攻击窗口内
        DashToPlayer();//向玩家位置突进
        EnableHitbox();
    }

    public void NoAttackWindow()
    {
        isAttackWindow = false;//目前不在动画事件的攻击窗口内
        DisableHitbox();
    }

    public void OnEnableTakeDamage()//允许对玩家造成伤害
    {
        isTakeDamage = false;
    }
    public void OnDisableDamage()
    {
        isTakeDamage = true;
    }
    public void UnAttack()
    {
        isAttack = false;
        animator.SetBool("IsAttack", isAttack);
        animator.SetBool("isMove", true);
    }
}