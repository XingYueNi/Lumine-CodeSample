using UnityEngine;

/// <summary>
/// 一次攻击的数值。挂在攻击方身上，逐个技能配置。
///
/// 为什么用 SO：策划/你自己调数值不用改代码、不用重新编译；
/// 同一份数据可以被多个敌人复用；改完在 Inspector 里立刻生效。
///
/// 创建：右键 Project → Create → Lumine → Combat → Attack Data
/// </summary>
[CreateAssetMenu(fileName = "NewAttack", menuName = "Lumine/Combat/Attack Data", order = 0)]
public class AttackData : ScriptableObject
{
    [Tooltip("备注名，仅用于在 Inspector 里辨认，比如「普攻一段」「横扫」")]
    public string displayName = "";

    [Header("伤害")]
    [Tooltip("造成的生命值伤害")]
    public float damage = 10f;

    [Tooltip("造成的架势伤害（打空对方架势条会破防，可被处决）")]
    public float postureDamage = 15f;

    [Header("被弹开时")]
    [Tooltip("这一击被对方完美弹反时，攻击方自己上涨的架势。0 = 不涨")]
    public float parrySelfPosture = 10f;
}