using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 处决（忍杀）标记。
///
/// 这个组件挂在 HDDUI 预制体下的 "ExecutionMarker" 节点上（见 Assets/Resources/UI/HDDUI.prefab），
/// 由工程现有的 UiManager 打开 HDDUI 时一起加载，**不自建 Canvas**。
///
/// 为什么是 UI 而不是挂敌人身上的 3D 物件：
///   只狼那个标记是 HUD 图标——每帧把敌人的世界坐标投影到屏幕再画，
///   所以不管敌人远近，图标在屏幕上大小恒定，也不会穿进模型。
///
/// 层级（运行时由代码生成子节点，美术资源用 ProceduralFx 程序化生成）：
///   ExecutionMarker  (本组件, 常驻激活)
///   └─ Marker        (显隐开关 + 呼吸动画)
///      ├─ Glow       暗红光斑
///      ├─ Ring       细亮红边
///      └─ Glyph      "忍"字
/// </summary>
public class ExecutionMarkerUI : MonoBehaviour
{
    private static ExecutionMarkerUI s_Instance;

    [Header("尺寸")]
    [Tooltip("标记直径（像素，按 1920x1080 参考分辨率）")]
    public float pixelSize = 64f;

    [Header("位置")]
    [Tooltip("相对锚点抬高的世界偏移，让标记落在胸口/头顶")]
    public Vector3 worldOffset = new Vector3(0f, 0.55f, 0f);

    [Header("相机")]
    [Tooltip("投影用的相机。留空时自动找：Camera.main → 任意启用的相机。\n" +
             "注意 Camera.main 依赖相机的 Tag 是 MainCamera，本工程 Level_1 的相机原本是 Untagged，\n" +
             "所以这里不强制依赖它。")]
    public Camera targetCamera;

    [Header("配色")]
    public Color glowColor = new Color(0.9f, 0.1f, 0.06f, 0.45f);
    public Color ringColor = new Color(1f, 0.22f, 0.12f, 0.95f);
    public Color glyphColor = new Color(1f, 0.35f, 0.25f, 1f);

    private RectTransform markerRoot;
    private Image glowImage;
    private Image ringImage;
    private TextMeshProUGUI glyph;
    private Canvas canvas;
    private bool warnedNoCamera;

    private Transform anchor;
    private Camera cam;
    private float time;
    private bool ready;

    private void Awake()
    {
        s_Instance = this;
        Build();
    }

    private void OnDestroy()
    {
        if (s_Instance == this) s_Instance = null;
    }

    // =====================================================================
    //  对外接口
    // =====================================================================

    public static void Show(Transform target)
    {
        if (s_Instance == null || target == null) return;
        s_Instance.anchor = target;
        if (s_Instance.markerRoot != null) s_Instance.markerRoot.gameObject.SetActive(true);
    }

    public static void Hide(Transform target)
    {
        if (s_Instance == null) return;
        // 只有当前锚点自己才能关掉，避免 A 关闭时把 B 的标记一起关掉
        if (target != null && s_Instance.anchor != target) return;

        s_Instance.anchor = null;
        if (s_Instance.markerRoot != null) s_Instance.markerRoot.gameObject.SetActive(false);
    }

    // =====================================================================
    //  构建（只建子节点，不建画布）
    // =====================================================================

    private void Build()
    {
        if (markerRoot == null)
        {
            GameObject go = new GameObject("Marker", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            markerRoot = (RectTransform)go.transform;
            markerRoot.anchorMin = new Vector2(0.5f, 0.5f);
            markerRoot.anchorMax = new Vector2(0.5f, 0.5f);
            markerRoot.pivot = new Vector2(0.5f, 0.5f);
            markerRoot.anchoredPosition = Vector2.zero;
        }

        glowImage = CreateImage("Glow", ProceduralFx.Glow, glowColor);
        ringImage = CreateImage("Ring", ProceduralFx.Ring, ringColor);
        glyph = CreateGlyph("Glyph", "忍", glyphColor);

        ready = true;
        ApplySize();
        markerRoot.gameObject.SetActive(false);
    }

    private Image CreateImage(string name, Texture2D tex, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(markerRoot, false);

        Image img = go.AddComponent<Image>();
        Sprite sp = ToSprite(tex);
        if (sp != null) img.sprite = sp;
        img.color = color;
        img.preserveAspect = true;
        img.raycastTarget = false;

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        return img;
    }

    private TextMeshProUGUI CreateGlyph(string name, string text, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(markerRoot, false);

        // 关键：先保持未激活，等字体赋完再启用。
        // TextMeshProUGUI 在 Awake/OnEnable 时会去取 TMP 默认字体（TMP Settings），
        // 本工程没导入 TMP Essentials，那里是空的，会当场刷
        // "The LiberationSans SDF Font Asset was not found"。
        // 未激活状态下 AddComponent 不会触发 Awake，等字体就位再激活，警告就不会出现。
        go.SetActive(false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = LoadFont();

        tmp.text = text;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = false;

        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        go.SetActive(true);   // 字体已就位，这时才让 TMP 初始化
        return tmp;
    }

    private void ApplySize()
    {
        if (!ready) return;

        if (markerRoot != null) markerRoot.sizeDelta = new Vector2(pixelSize, pixelSize);

        if (ringImage != null)
        {
            ((RectTransform)ringImage.transform).sizeDelta = new Vector2(pixelSize, pixelSize);
        }
        if (glowImage != null)
        {
            float g = pixelSize * 1.55f;
            ((RectTransform)glowImage.transform).sizeDelta = new Vector2(g, g);
        }
        if (glyph != null)
        {
            glyph.rectTransform.sizeDelta = new Vector2(pixelSize, pixelSize);
            glyph.fontSize = Mathf.Max(8f, pixelSize * 0.62f);
        }
    }

    // =====================================================================
    //  每帧跟随
    // =====================================================================

    private void LateUpdate()
    {
        if (!ready) return;

        if (anchor == null)
        {
            if (markerRoot.gameObject.activeSelf) markerRoot.gameObject.SetActive(false);
            return;
        }

        Camera viewCam = ResolveCamera();
        if (viewCam == null) return;

        Canvas uiCanvas = ResolveCanvas();
        if (uiCanvas == null) return;

        Vector3 world = anchor.position + worldOffset;
        Vector3 screen = viewCam.WorldToScreenPoint(world);
        if (screen.z <= 0f)
        {
            if (markerRoot.gameObject.activeSelf) markerRoot.gameObject.SetActive(false);
            return;
        }

        // anchoredPosition 是相对「父节点矩形」的坐标。
        // 原实现算的是根画布的局部坐标却赋给了 markerRoot，两者原点不一致时
        // 标记就会一直贴在屏幕正中——这是"位置永远在中间"的根因。
        RectTransform parentRect = markerRoot.parent as RectTransform;
        if (parentRect == null)
        {
            Canvas rootCanvas = uiCanvas.rootCanvas != null ? uiCanvas.rootCanvas : uiCanvas;
            parentRect = (RectTransform)rootCanvas.transform;
        }

        // Overlay 画布：屏幕像素即画布像素，相机传 null；
        // Screen Space - Camera / World Space：必须传画布自己的相机，否则换算结果错。
        Camera uiCam = uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCanvas.worldCamera;

        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screen, uiCam, out local))
            return;

        if (!markerRoot.gameObject.activeSelf) markerRoot.gameObject.SetActive(true);
        markerRoot.anchoredPosition = local;

        // 呼吸
        time += Time.unscaledDeltaTime;
        float pulse = 1f + Mathf.Sin(time * 3f) * 0.06f;
        markerRoot.localScale = Vector3.one * pulse;

        if (ringImage != null)
        {
            Color c = ringColor;
            c.a = Mathf.Lerp(0.7f, 1f, (Mathf.Sin(time * 3f) + 1f) * 0.5f);
            ringImage.color = c;
        }
    }

    // =====================================================================
    //  依赖解析（每帧兜底，允许相机/画布晚于本组件创建）
    // =====================================================================

    private Camera ResolveCamera()
    {
        if (targetCamera != null) return targetCamera;
        if (cam != null) return cam;

        // 1. 标准做法：依赖 Tag = MainCamera
        cam = Camera.main;

        // 2. 退路：任何启用中的相机。
        //    本工程 Level_1 的相机 Tag 是 Untagged，Camera.main 会返回 null，
        //    没有这一步标记就永远停在屏幕中心。
        if (cam == null)
        {
            Camera[] all = Camera.allCameras;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].isActiveAndEnabled)
                {
                    cam = all[i];
                    break;
                }
            }
        }

        if (cam == null && !warnedNoCamera)
        {
            warnedNoCamera = true;
            Debug.LogWarning("[处决标记] 找不到可用相机，标记无法投影。请在 ExecutionMarker 节点上手动指定 targetCamera。");
        }
        return cam;
    }

    private Canvas ResolveCanvas()
    {
        if (canvas != null) return canvas;

        canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        return canvas;
    }

    /// <summary>找字体：优先工程自带的思源黑体，退化到 TMP 默认字体</summary>
    private static TMP_FontAsset LoadFont()
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/思源黑体 SDF");
        if (font == null) font = TMP_Settings.defaultFontAsset;
        if (font == null)
            Debug.LogWarning("[处决标记] 找不到字体（Resources/Fonts/思源黑体 SDF 与 TMP 默认字体均为空），标记里的字不会显示。");
        return font;
    }

    private static Sprite ToSprite(Texture2D tex)
    {
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }
}
