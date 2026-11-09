using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public List<Transform> enemies = new List<Transform>();

    void Awake()
    {
        // 优先用自己（本组件通常就挂在 Enemy 物体上）；
        // 找不到时退化为 this，避免 Instance 变成 null 让后面所有调用炸掉
        GameObject go = GameObject.Find("Enemy");
        if (go != null) Instance = go.GetComponent<EnemyManager>();
        if (Instance == null) Instance = this;
    }

    public void RegisterEnemy(Transform enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    public void UnregisterEnemy(Transform enemy)
    {
        enemies.Remove(enemy);
        enemies.RemoveAll(t => t == null);
    }

    public List<Transform> GetAllEnemies()
    {
        // 敌人被 Destroy 后，它的 Transform 还留在这个列表里，
        // 变成「已销毁的引用」——访问 position 会抛 MissingReferenceException。
        // 取列表时顺手清掉，成本很低。
        enemies.RemoveAll(t => t == null);
        return enemies;
    }
}
