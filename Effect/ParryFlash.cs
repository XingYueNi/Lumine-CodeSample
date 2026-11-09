using UnityEngine;

/// <summary>
/// 弹反特效（只狼风格火花飞溅）。
///
/// 结构从内到外分五层，全部代码生成，不依赖美术资源：
///   1. 白热核心   —— 极短的纯白爆闪，撑住"打铁"那一瞬间的亮
///   2. 橙色光晕   —— 大一圈的暖光，给画面一层热浪感
///   3. 冲击亮环   —— 向外炸开的细环
///   4. 火星三层   —— 高速白热长条 / 中速橙火 / 低速慢坠余烬
///   5. 斜向拉丝   —— 沿兵器交击方向的两道长条，模拟刀锋擦出的亮线
///
/// 用法：
///   ParryFlash.Spawn(hitPosition);
///   ParryFlash.Spawn(hitPosition, clashDirection, 1.2f);
/// </summary>
public class ParryFlash : MonoBehaviour
{
    /// <summary>生成一次弹反火花</summary>
    // 去重：同一次交击可能被多个入口各调用一次（玩家弹反 / 被弹开 / 敌人弹反），
    // 这里把「同一时刻 + 很近的位置」合并成一次，避免同一瞬间炸出好几团。
    private static float s_LastSpawnTime = -999f;
    private static Vector3 s_LastSpawnPos;
    private const float DedupeWindow = 0.12f;
    private const float DedupeRadius = 1.2f;

    public static ParryFlash Spawn(Vector3 position, Vector3 direction = default(Vector3), float scale = 1f)
    {
        if (Time.time - s_LastSpawnTime < DedupeWindow &&
            Vector3.Distance(position, s_LastSpawnPos) < DedupeRadius)
        {
            return null;
        }
        s_LastSpawnTime = Time.time;
        s_LastSpawnPos = position;

        GameObject go = new GameObject("ParryFlash");
        go.transform.position = position;

        ParryFlash fx = go.AddComponent<ParryFlash>();
        fx.Build(direction, Mathf.Max(0.1f, scale));
        return fx;
    }

    [Header("时长")]
    public float lifeTime = 1.1f;

    [Header("配色（只狼是暖橙，想换冷色改这里）")]
    public Color coreColor = new Color(1f, 0.98f, 0.92f, 1f);   // 白热核心
    public Color glowColor = new Color(1f, 0.55f, 0.12f, 1f);   // 橙色光晕
    public Color hotSparkColor = new Color(1f, 0.85f, 0.45f, 1f); // 高速火星
    public Color emberColor = new Color(1f, 0.38f, 0.05f, 1f);   // 余烬
    public Color ringColor = new Color(1f, 0.72f, 0.3f, 1f);     // 亮环

    [Header("点光")]
    public bool useLight = true;

    [Header("火星数量")]
    public int hotSparkCount = 70;
    public int emberCount = 45;
    public int streakCount = 8;

    private float elapsed;
    private Light flashLight;
    private float lightIntensity;

    private void Build(Vector3 direction, float scale)
    {
        if (direction.sqrMagnitude > 0.001f)
            transform.position += direction.normalized * 0.02f;

        BuildCore(scale);
        BuildGlow(scale);
        BuildRing(scale);
        BuildHotSparks(scale);
        BuildEmbers(scale);
        BuildStreaks(scale, direction);

        if (useLight) BuildLight(scale);
    }

    // ---------------- 1. 白热核心 ----------------

    private void BuildCore(float scale)
    {
        ParticleSystem ps = NewSystem("Core", ProceduralFx.Glow);
        if (ps == null) return;

        var main = ps.main;
        main.startLifetime = 0.09f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.9f * scale);
        main.startSpeed = 0f;
        main.startColor = coreColor;
        main.maxParticles = 1;

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.55f, 1f, 1.8f));

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(FadeGradient(coreColor, 1f, 0f));

        Burst(ps, 1);
    }

    // ---------------- 2. 橙色光晕 ----------------

    private void BuildGlow(float scale)
    {
        ParticleSystem ps = NewSystem("Glow", ProceduralFx.Glow);
        if (ps == null) return;

        var main = ps.main;
        main.startLifetime = 0.34f;
        main.startSize = new ParticleSystem.MinMaxCurve(1.5f * scale);
        main.startSpeed = 0f;
        main.startColor = glowColor;
        main.maxParticles = 1;

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.7f, 1f, 2.4f));

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(FadeGradient(glowColor, 0.85f, 0f));

        Burst(ps, 1);
    }

    // ---------------- 3. 冲击亮环 ----------------

    private void BuildRing(float scale)
    {
        ParticleSystem ps = NewSystem("Ring", ProceduralFx.Ring);
        if (ps == null) return;

        var main = ps.main;
        main.startLifetime = 0.3f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f * scale);
        main.startSpeed = 0f;
        main.startColor = ringColor;
        main.maxParticles = 1;

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.35f, 1f, 3.2f));

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(FadeGradient(ringColor, 1f, 0f));

        Burst(ps, 1);
    }

    // ---------------- 4a. 高速白热火星 ----------------

    private void BuildHotSparks(float scale)
    {
        ParticleSystem ps = NewSystem("HotSparks", ProceduralFx.Glow);
        if (ps == null) return;

        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(7f * scale, 15f * scale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.012f * scale, 0.03f * scale);
        main.startColor = hotSparkColor;
        main.gravityModifier = 1.1f;
        main.maxParticles = 220;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

        // 球面发射：从一点向四周均匀炸开
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.03f * scale;
        shape.radiusThickness = 0f;      // 0 = 只在球面上发射，才是放射状

        // 速度上限 + 阻尼：火星刚炸出去很快，随即急刹
        var limit = ps.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.limit = 1.2f;
        limit.dampen = 0.55f;
        limit.multiplyDragByParticleSize = false;

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.25f));

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(
            HotGradient(hotSparkColor, new Color(1f, 0.35f, 0.05f, 1f)));

        ParticleSystemRenderer pr = ps.GetComponent<ParticleSystemRenderer>();
        if (pr != null)
        {
            pr.renderMode = ParticleSystemRenderMode.Stretch;
            pr.velocityScale = 0.09f;
            pr.lengthScale = 3.2f;
            pr.maxParticleSize = 1f;
        }

        Burst(ps, hotSparkCount);
    }

    // ---------------- 4b. 慢坠余烬 ----------------

    private void BuildEmbers(float scale)
    {
        ParticleSystem ps = NewSystem("Embers", ProceduralFx.Glow);
        if (ps == null) return;

        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f * scale, 5f * scale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f * scale, 0.05f * scale);
        main.startColor = emberColor;
        main.gravityModifier = 2.2f;
        main.maxParticles = 160;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f * scale;
        shape.radiusThickness = 0f;

        var limit = ps.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.limit = 0.6f;
        limit.dampen = 0.5f;

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.1f));

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(
            HotGradient(emberColor, new Color(0.5f, 0.08f, 0.02f, 1f)));

        ParticleSystemRenderer pr = ps.GetComponent<ParticleSystemRenderer>();
        if (pr != null)
        {
            pr.renderMode = ParticleSystemRenderMode.Stretch;
            pr.velocityScale = 0.12f;
            pr.lengthScale = 2.2f;
            pr.maxParticleSize = 1f;
        }

        Burst(ps, emberCount);
    }

    // ---------------- 5. 交击拉丝 ----------------

    private void BuildStreaks(float scale, Vector3 direction)
    {
        // 没有给方向就朝着摄像机右方拉，保证总能看到横向亮线
        Vector3 dir = direction;
        if (dir.sqrMagnitude < 0.001f)
        {
            Camera cam = Camera.main;
            dir = cam != null ? cam.transform.right : Vector3.right;
        }
        dir.Normalize();

        ParticleSystem ps = NewSystem("Streaks", ProceduralFx.Glow);
        if (ps == null) return;

        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.16f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(16f * scale, 26f * scale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f * scale, 0.04f * scale);
        main.startColor = new Color(1f, 0.9f, 0.7f, 1f);
        main.maxParticles = 40;
        main.gravityModifier = 0f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 4f;
        shape.radius = 0.01f;
        // Cone 的朝向跟物体旋转走，这里直接把发射体转到交击方向上
        shape.rotation = Quaternion.LookRotation(dir).eulerAngles;

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f));

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(FadeGradient(new Color(1f, 0.92f, 0.75f, 1f), 1f, 0f));

        ParticleSystemRenderer pr = ps.GetComponent<ParticleSystemRenderer>();
        if (pr != null)
        {
            pr.renderMode = ParticleSystemRenderMode.Stretch;
            pr.velocityScale = 0.05f;
            pr.lengthScale = 9f;         // 拉得很长，像一道亮线
            pr.maxParticleSize = 2f;
        }

        // 分两批发射，中间隔 0.01 秒，让亮线有先后层次
        int half = Mathf.Max(2, streakCount / 2);
        SetBursts(ps, new[]
        {
            new ParticleSystem.Burst(0f, (short)half),
            new ParticleSystem.Burst(0.01f, (short)half)
        });
    }

    // ---------------- 点光 ----------------

    private void BuildLight(float scale)
    {
        flashLight = gameObject.AddComponent<Light>();
        flashLight.type = LightType.Point;
        flashLight.color = glowColor;
        flashLight.range = 5f * scale;
        flashLight.shadows = LightShadows.None;

        lightIntensity = 7f * scale;
        flashLight.intensity = lightIntensity;
    }

    // ---------------- 生命周期 ----------------

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (flashLight != null)
        {
            float k = 1f - Mathf.Clamp01(elapsed / 0.14f);
            flashLight.intensity = lightIntensity * k * k;
        }

        if (elapsed >= lifeTime) Destroy(gameObject);
    }

    // ---------------- 工具 ----------------

    private ParticleSystem NewSystem(string name, Texture2D tex)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        // 必须先停再配置：AddComponent 得到的粒子系统默认就在播放，
        // 播放中修改 duration 会持续刷 "Setting the duration while system is still playing"。
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // ⚠ 关键：新建的粒子系统 Shape 模块默认是「半径 1 的圆锥」，
        // 粒子会从半径 1 米处的随机点发射，本该在正中心的层（核心/光晕/冲击环）
        // 会整体偏移出去。这里默认关掉，需要形状的层（火星 等）自己再打开配置。
        var shapeModule = ps.shape;
        shapeModule.enabled = false;

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.05f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        var emission = ps.emission;
        emission.rateOverTime = 0f;

        ParticleSystemRenderer pr = ps.GetComponent<ParticleSystemRenderer>();
        if (pr != null)
        {
            Material mat = ProceduralFx.NewMaterial(tex);
            if (mat != null) pr.sharedMaterial = mat;
            pr.renderMode = ParticleSystemRenderMode.Billboard;
            pr.alignment = ParticleSystemRenderSpace.View;
            pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            pr.receiveShadows = false;
        }

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return ps;
    }

    private static void Burst(ParticleSystem ps, int count)
    {
        SetBursts(ps, new[] { new ParticleSystem.Burst(0f, (short)count) });
    }

    /// <summary>
    /// 一次性把 burst 表设好。
    /// 注意：不要用 em.GetBursts() 无参调用——某些 Unity 版本只提供
    /// GetBursts(ParticleSystem.Burst[] bursts) 这个带缓冲区的重载。
    /// </summary>
    private static void SetBursts(ParticleSystem ps, ParticleSystem.Burst[] bursts)
    {
        var em = ps.emission;
        em.enabled = true;
        em.rateOverTime = 0f;
        em.SetBursts(bursts);

        if (!ps.isPlaying) ps.Play();
    }

    private static Gradient FadeGradient(Color c, float startAlpha, float endAlpha)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(c.r, c.g, c.b), 0f),
                new GradientColorKey(new Color(c.r, c.g, c.b), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(startAlpha, 0f),
                new GradientAlphaKey(startAlpha * 0.75f, 0.35f),
                new GradientAlphaKey(endAlpha, 1f)
            });
        return g;
    }

    private static Gradient HotGradient(Color from, Color to)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(from.r, from.g, from.b), 0f),
                new GradientColorKey(new Color(to.r, to.g, to.b), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.9f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            });
        return g;
    }
}
