using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 武器挥砍的空气轨迹（刀光拖尾）。
///
/// 原理：TrailRenderer 跟着刀尖走，把最近一小段时间内刀尖划过的位置连成一条带子。
/// 攻击窗口开时开始发射、关时停止发射，带子自然淡出。
///
/// 用法（最省事）：
///   挂在 Player（或任意挂着 PlayerManager 的物体）上，什么都不用配。
///   Awake 时会自动从 PlayerManager.raycastPoints 里取离武器根最远的那个点当刀尖。
///
/// 也可以手动指定 bladeTip（比如给敌人用，敌人身上没有 PlayerManager）。
/// </summary>
public class SwordTrail : MonoBehaviour
{
    [Header("刀尖（留空则自动从 PlayerManager 取刀身最末端）")]
    public Transform bladeTip;

    [Header("拖尾参数")]
    [Tooltip("拖尾保留时长，越大越长")]
    public float trailTime = 0.11f;
    [Tooltip("靠近刀尖一端的宽度")]
    public float widthFrom = 0.05f;
    [Tooltip("远离刀尖一端的宽度（弧线尾端）")]
    public float widthTo = 0.32f;
    [Tooltip("相邻顶点的最小间距，太小会浪费顶点")]
    public float minVertexDistance = 0.02f;

    [Header("颜色")]
    public Color tipColor = new Color(0.85f, 0.93f, 1f, 0.95f);
    public Color tailColor = new Color(0.55f, 0.75f, 1f, 0f);

    private TrailRenderer trail;
    private Material trailMaterial;

    private void Awake()
    {
        if (bladeTip == null) bladeTip = ResolveTip();
        BuildTrail();
    }

    /// <summary>
    /// 自动找刀尖：从 PlayerManager / Enemy 的 raycastPoints 里取离角色最远的那个点，
    /// 也就是刀身末端。
    /// （这两个类各自声明了 raycastPoints，基类 CharacterCombat 里没有。）
    /// </summary>
    private Transform ResolveTip()
    {
        Transform[] points = null;

        PlayerManager pm = GetComponent<PlayerManager>();
        if (pm != null) points = pm.raycastPoints;

        if (points == null || points.Length == 0)
        {
            Enemy enemy = GetComponent<Enemy>();
            if (enemy != null) points = enemy.raycastPoints;
        }

        if (points != null && points.Length > 0)
        {
            Transform best = null;
            float bestDist = -1f;
            for (int i = 0; i < points.Length; i++)
            {
                Transform p = points[i];
                if (p == null) continue;
                float d = Vector3.Distance(p.position, transform.position);
                if (d > bestDist) { bestDist = d; best = p; }
            }
            if (best != null) return best;
        }

        Debug.LogWarning("[刀光] 没能自动找到刀尖，请在 SwordTrail 上手动指定 bladeTip。", this);
        return transform;
    }

    private void BuildTrail()
    {
        if (bladeTip == null) return;

        // 拖尾挂在刀尖的子节点上，避免影响刀尖本身的 Transform
        GameObject go = new GameObject("SwordTrailRibbon");
        go.transform.SetParent(bladeTip, false);
        go.transform.localPosition = Vector3.zero;

        trailMaterial = ProceduralFx.NewMaterial(ProceduralFx.Glow);

        trail = go.AddComponent<TrailRenderer>();
        trail.sharedMaterial = trailMaterial;

        trail.time = trailTime;
        trail.minVertexDistance = minVertexDistance;
        trail.autodestruct = false;
        trail.emitting = false;

        // 宽度：刀尖端窄、弧尾宽，像一道扫出去的风
        AnimationCurve width = new AnimationCurve(
            new Keyframe(0f, widthFrom),
            new Keyframe(0.55f, widthTo),
            new Keyframe(1f, widthTo * 0.35f));
        trail.widthCurve = width;
        trail.widthMultiplier = 1f;

        // 颜色：刀尖白亮 -> 尾部透明淡蓝
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(tipColor.r, tipColor.g, tipColor.b), 0f),
                new GradientColorKey(new Color(tailColor.r, tailColor.g, tailColor.b), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(tipColor.a, 0f),
                new GradientAlphaKey(tipColor.a * 0.6f, 0.35f),
                new GradientAlphaKey(0f, 1f)
            });
        trail.colorGradient = g;

        // 面向摄像机，弧线在屏幕上永远是一条平顺的带子
        trail.alignment = LineAlignment.View;
        trail.textureMode = LineTextureMode.Stretch;
        trail.numCapVertices = 2;
        trail.numCornerVertices = 2;
        trail.shadowCastingMode = ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.generateLightingData = false;
    }

    /// <summary>攻击窗口开始（由动画事件 EnableHitbox 调用）</summary>
    public void Begin()
    {
        if (trail == null)
        {
            if (bladeTip == null) bladeTip = ResolveTip();
            BuildTrail();
        }
        if (trail == null) return;

        trail.Clear();
        trail.emitting = true;
    }

    /// <summary>攻击窗口结束（由动画事件 DisableHitbox 调用）</summary>
    public void End()
    {
        if (trail == null) return;
        trail.emitting = false;
        // 不立刻 Clear，让已有的一段自然淡出
    }

    private void OnDestroy()
    {
        if (trail != null && trail.gameObject != null) Destroy(trail.gameObject);
        if (trailMaterial != null) Destroy(trailMaterial);
    }
}
