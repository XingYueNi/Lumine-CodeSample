using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敌人警戒条。读自己所属敌人的 alertValue，不再读全局单例。
/// </summary>
public class EnemyAlert : MonoBehaviour
{
    public Image image;

    private Enemy enemy;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();

        if (enemy == null)
        {
            Debug.LogWarning("[EnemyAlert] 没找到父级 Enemy，退回到 Enemy.Instance。");
            enemy = Enemy.Instance;
        }
    }

    private void Update()
    {
        if (image == null || enemy == null) return;
        image.fillAmount = enemy.alertValue / 100f;
    }
}