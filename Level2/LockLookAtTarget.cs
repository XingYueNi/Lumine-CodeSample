using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockLookAtTarget : MonoBehaviour
{
    private Transform lockLookAtTarget;
    public Transform player;
    private Transform lockEnemy;
    private void Awake()
    {
        lockLookAtTarget = transform;
    }
    private void LateUpdate()
    {
        UpdateLockLookAt();
    }
    private void OnEnable()
    {
        PlayerControl.OnLockEnemyChanged += GetLockEnemy ;
    }
    private void GetLockEnemy(Transform target)
    {
        lockEnemy = target;
        //Debug.Log("已锁定敌人");
    }
    private void UpdateLockLookAt()
    {
        if (lockEnemy == null || lockLookAtTarget == null) return;

        // 取玩家和敌人的胸口高度位置
        Vector3 playerChest = player.transform.position + Vector3.up * 1.2f;
        Vector3 enemyChest = lockEnemy.position + Vector3.up * 1.2f;

        float dist = Vector3.Distance(playerChest, enemyChest);

        // 距离越近，中点越偏向玩家前方，避免敌人贴脸时相机俯视
        // dist = 5 米时 t = 1（完全用中点）
        // dist = 1 米时 t = 0.2（几乎只看玩家前方）
        float t = Mathf.Clamp01(dist / 5f);
        t = Mathf.Lerp(0.3f, 1f, t);

        Vector3 midpoint = Vector3.Lerp(playerChest, enemyChest, 0.5f * t);

        // 高度锁定在玩家胸口，不要因为敌人矮/高就上下移动
        midpoint.y = playerChest.y;

        lockLookAtTarget.position = midpoint;
    }
}
