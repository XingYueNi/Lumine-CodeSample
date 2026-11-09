using UnityEngine;

/// <summary>
/// 程序化生成的贴图与材质，不依赖任何美术资源。
///
/// 用途：工程在打包精简时清掉了 Resources/eff，没有现成的火花/刀光贴图，
/// 所以这里用代码生成两张贴图：
///   Glow —— 中心亮、边缘透明的柔光圆，用于弹反闪光和刀光拖尾
///   Ring —— 中间镂空的环，用于弹反瞬间向外扩散的冲击环
///
/// 生成的贴图会被缓存，只在第一次使用时创建。
/// </summary>
public static class ProceduralFx
{
    private static Texture2D s_Glow;
    private static Texture2D s_Ring;
    private static Shader s_SpriteShader;

    /// <summary>柔光圆（白，alpha 由中心向外衰减）</summary>
    public static Texture2D Glow
    {
        get
        {
            if (s_Glow == null) s_Glow = BuildGlow(128);
            return s_Glow;
        }
    }

    /// <summary>冲击环（白，中间透明、环带不透明、外缘衰减）</summary>
    public static Texture2D Ring
    {
        get
        {
            if (s_Ring == null) s_Ring = BuildRing(256);
            return s_Ring;
        }
    }

    /// <summary>
    /// 取一个带 alpha 混合的着色器。
    /// Sprites/Default 默认就在 Always Included Shaders 里，
    /// 打包不会丢，而且会正确乘上粒子系统烘进顶点色的颜色。
    /// </summary>
    public static Shader BlendShader
    {
        get
        {
            if (s_SpriteShader == null)
            {
                s_SpriteShader = Shader.Find("Sprites/Default");
                if (s_SpriteShader == null) s_SpriteShader = Shader.Find("Universal Render Pipeline/Unlit");
                if (s_SpriteShader == null) s_SpriteShader = Shader.Find("Unlit/Transparent");
            }
            return s_SpriteShader;
        }
    }

    /// <summary>创建一份材质（调用方负责在合适时机销毁）</summary>
    public static Material NewMaterial(Texture2D tex)
    {
        Shader sh = BlendShader;
        if (sh == null)
        {
            Debug.LogError("[特效] 找不到可用的 alpha 混合着色器，特效会不可见。");
            return null;
        }

        Material m = new Material(sh);
        if (tex != null) m.mainTexture = tex;
        if (m.HasProperty("_Color")) m.SetColor("_Color", Color.white);
        return m;
    }

    // ---------------- 贴图生成 ----------------

    private static Texture2D BuildGlow(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float half = (size - 1) * 0.5f;
        Color32[] px = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;
                float r = Mathf.Sqrt(dx * dx + dy * dy);

                // 中心 1，半径 1 处 0，用四次方让边缘衰减更柔和
                float a = Mathf.Clamp01(1f - r);
                a = a * a * a * a;

                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }

        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    private static Texture2D BuildRing(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float half = (size - 1) * 0.5f;
        Color32[] px = new Color32[size * size];

        // 环带中心半径 0.78，带宽 0.16
        const float ringCenter = 0.78f;
        const float ringWidth = 0.16f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;
                float r = Mathf.Sqrt(dx * dx + dy * dy);

                // 到环带中心的距离，映射成 1 -> 0
                float d = Mathf.Abs(r - ringCenter) / ringWidth;
                float a = Mathf.Clamp01(1f - d);
                a = a * a;                       // 软边
                if (r > 1.02f) a = 0f;           // 环外彻底透明

                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }

        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }
}
