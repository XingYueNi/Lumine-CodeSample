using System;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveDataCell : MonoBehaviour
{
    private GameObject thumbnailUI;
    private int index;

    private void Awake()
    {
        thumbnailUI = transform.Find("thumbnail").gameObject;
    }

    public void Init(int index)
    {
        this.index = index;

        // 读这个槽位的数据（不动 currentData，避免影响正在进行的游戏）
        SaveData data = SaveDataManager.Instance.PeekSaveData(index);
        RefreshTexts(data);
        RefreshThumbnail(index);

        // 无论有没有缩略图都要能点，否则没截图的存档点不了
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveListener(LoadSave);
        btn.onClick.AddListener(LoadSave);
    }

    // ---------------- 文字 ----------------

    private void RefreshTexts(SaveData data)
    {
        TextMeshProUGUI playTxt = FindText("PlayGameTime");
        if (playTxt != null)
            playTxt.text = "游玩时间：" + FormatPlayTime(data != null ? data.playSeconds : 0f);

        TextMeshProUGUI lastTxt = FindText("LastPlayTime");
        if (lastTxt != null)
            lastTxt.text = "上一次游玩：" + FormatSaveTime(data != null ? data.saveTimeIso : null);
    }

    private static string FormatPlayTime(float seconds)
    {
        if (seconds < 60f) return "不到 1 分钟";

        int total = Mathf.FloorToInt(seconds);
        int hours = total / 3600;
        int minutes = (total % 3600) / 60;

        if (hours > 0) return hours + " 小时 " + minutes + " 分";
        return minutes + " 分钟";
    }

    private static string FormatSaveTime(string iso)
    {
        if (string.IsNullOrEmpty(iso)) return "未知";

        DateTime t;
        if (!DateTime.TryParse(iso, null, DateTimeStyles.RoundtripKind, out t)) return "未知";

        TimeSpan span = DateTime.Now - t;
        if (span.TotalSeconds < 0) return "未知";             // 系统时间被调乱时不硬编
        if (span.TotalMinutes < 1) return "刚刚";
        if (span.TotalHours < 1) return Mathf.Max(1, (int)span.TotalMinutes) + " 分钟前";
        if (span.TotalDays < 1) return "今天 " + t.ToString("HH:mm");
        if (span.TotalDays < 2) return "昨天 " + t.ToString("HH:mm");
        if (span.TotalDays < 30) return (int)span.TotalDays + " 天前";
        return t.ToString("yyyy-MM-dd");
    }

    private TextMeshProUGUI FindText(string objectName)
    {
        TextMeshProUGUI[] all = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < all.Length; i++)
            if (all[i].name == objectName) return all[i];
        return null;
    }

    // ---------------- 缩略图 ----------------

    private void RefreshThumbnail(int index)
    {
        if (thumbnailUI == null) return;

        RawImage raw = thumbnailUI.GetComponent<RawImage>();
        string path = SaveDataManager.Instance.GetScreenshotPath(index);

        if (!File.Exists(path))
        {
            raw.texture = null;
            return;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            raw.texture = tex;
        }
        catch (Exception e)
        {
            Debug.LogWarning("存档缩略图读取失败：" + e.Message);
            raw.texture = null;
        }
    }

    private void LoadSave()
    {
        SaveDataManager.Instance.Load(index);
    }
}