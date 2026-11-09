using UnityEngine;

/// <summary>
/// 敌人血条。绑定到「自己所属的那个敌人」，而不是全局单例。
///
/// 原来写的是 Enemy.Instance.OnHealthChanged，但 Enemy.Instance 是静态变量，
/// 最终只会指向最后一个 Awake 的敌人 —— 场上有多个敌人时，所有血条都在看同一个，
/// 你打的那只血条自然不动。
/// </summary>
public class EnemyHpLarp : HPImageLarp
{
    private Enemy enemy;

    protected void Start()
    {
        enemy = GetComponentInParent<Enemy>();

        if (enemy == null)
        {
            Debug.LogWarning("[EnemyHpLarp] 没找到父级 Enemy，退回到 Enemy.Instance。请确认血条挂在敌人预制体下面。");
            enemy = Enemy.Instance;
        }

        if (enemy != null) enemy.OnHealthChanged += OnHpChange;
    }

    private void OnDestroy()
    {
        if (enemy != null) enemy.OnHealthChanged -= OnHpChange;
    }
}