using UnityEngine;

/// <summary>
/// 敌人架势条。同样绑定到自己的父级 Enemy，不再用全局单例。
/// </summary>
public class EnemySTLarp : STLarp
{
    private Enemy enemy;

    protected void Start()
    {
        enemy = GetComponentInParent<Enemy>();

        if (enemy == null)
        {
            Debug.LogWarning("[EnemySTLarp] 没找到父级 Enemy，退回到 Enemy.Instance。请确认架势条挂在敌人预制体下面。");
            enemy = Enemy.Instance;
        }

        if (enemy != null) enemy.OnPostureChanged += OnPosture;
    }

    private void OnDestroy()
    {
        if (enemy != null) enemy.OnPostureChanged -= OnPosture;
    }
}