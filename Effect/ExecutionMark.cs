using UnityEngine;

/// <summary>
/// 挂在 Enemy.executionPrompt 指向的那个物体上（原来是用来占位的球）。
///
/// 职责：
///   1. Awake 里把占位球的网格 / 渲染 / 碰撞体关掉；
///   2. OnEnable / OnDisable 通知 HDDUI 里的 ExecutionMarkerUI 显示 / 隐藏屏幕标记。
///
/// 显示时机仍由 Enemy.WaitExecution(bool) 的 SetActive 控制，这里不判断游戏逻辑。
/// 标记的尺寸和偏移在 HDDUI 预制体的 ExecutionMarker 节点上配置。
/// </summary>
public class ExecutionMark : MonoBehaviour
{
    private void Awake()
    {
        HidePlaceholder();
    }

    private void OnEnable()
    {
        ExecutionMarkerUI.Show(transform);
    }

    private void OnDisable()
    {
        ExecutionMarkerUI.Hide(transform);
    }

    /// <summary>把原先用来占位的球（MeshFilter / MeshRenderer / Collider）关掉</summary>
    private void HidePlaceholder()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null) mf.sharedMesh = null;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}
