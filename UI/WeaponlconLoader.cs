using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class WeaponIconLoader : MonoBehaviour
{
    public static WeaponIconLoader Instance { get; private set; }

    [Header("图集资源名（Resources 路径）")]
    public string atlasResourceName = "IconsAtlas/WeaponIconsAtlas";

    private SpriteAtlas atlas;
    private Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadAtlas();
    }

    private void LoadAtlas()
    {
        atlas = Resources.Load<SpriteAtlas>(atlasResourceName);

        if (atlas == null)
            Debug.LogError($"图集加载失败：{atlasResourceName}");
        else
            Debug.Log($"图集加载成功，包含 {atlas.spriteCount} 个 Sprite");
    }

    public Sprite GetIcon(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return null;

        if (cache.TryGetValue(spriteName, out var cached))
            return cached;

        if (atlas == null)
        {
            Debug.LogError("图集未加载");
            return null;
        }

        Sprite sprite = atlas.GetSprite(spriteName);
        if (sprite == null)
        {
            Debug.LogError($"图集里找不到 Sprite：{spriteName}");
            return null;
        }

        cache[spriteName] = sprite;
        return sprite;
    }
}