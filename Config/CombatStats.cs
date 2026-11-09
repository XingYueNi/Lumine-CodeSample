using UnityEngine;

/// <summary>
/// 一个角色的战斗基础属性。玩家和敌人各用一份。
///
/// 创建：右键 Project → Create → Lumine → Combat → Combat Stats
/// </summary>
[CreateAssetMenu(fileName = "NewCombatStats", menuName = "Lumine/Combat/Combat Stats", order = 1)]
public class CombatStats : ScriptableObject
{
    [Header("生命")]
    public float maxHealth = 100f;

    [Header("架势")]
    [Tooltip("架势条上限。打满即破防，可被处决")]
    public float maxPosture = 100f;

    [Tooltip("每秒自动恢复的架势值")]
    public float postureRecoverySpeed = 10f;

    [Tooltip("最后一次受击后，隔多久才开始恢复架势")]
    public float postureRecoveryDelay = 1.5f;

    [Header("防御")]
    [Tooltip("普通格挡时受到的伤害倍率。0.1 = 只吃 10% 伤害")]
    [Range(0f, 1f)]
    public float blockDamageMultiplier = 0.1f;

    [Tooltip("普通格挡时架势上涨的倍率。0.5 = 只涨一半")]
    [Range(0f, 2f)]
    public float blockPostureMultiplier = 0.5f;

    [Header("弹反与破防")]
    [Tooltip("完美弹反的时间窗口（秒）")]
    public float parryWindow = 0.5f;

    [Tooltip("破防状态持续多久（秒），期间可被处决")]
    public float staggerDuration = 4f;
}