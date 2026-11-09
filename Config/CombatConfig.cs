using UnityEngine;

/// <summary>
/// 战斗数值的统一加载入口。
///
/// 规则：组件上拖了 SO 就用拖进来的；没拖就从 Resources/Combat/ 下按名字加载。
/// 这样即使忘了拖引用，游戏也不会因为空引用崩掉，只会用代码里的默认值兜底。
/// </summary>
public static class CombatConfig
{
    public const string ResourceFolder = "Combat/";

    public static AttackData LoadAttack(string assetName)
    {
        if (string.IsNullOrEmpty(assetName)) return null;

        AttackData data = Resources.Load<AttackData>(ResourceFolder + assetName);
        if (data == null)
        {
            Debug.LogWarning("[CombatConfig] 找不到攻击数据 Resources/" + ResourceFolder + assetName + "，使用代码默认值。");
        }
        return data;
    }

    public static CombatStats LoadStats(string assetName)
    {
        if (string.IsNullOrEmpty(assetName)) return null;

        CombatStats stats = Resources.Load<CombatStats>(ResourceFolder + assetName);
        if (stats == null)
        {
            Debug.LogWarning("[CombatConfig] 找不到角色属性 Resources/" + ResourceFolder + assetName + "，使用代码默认值。");
        }
        return stats;
    }
}