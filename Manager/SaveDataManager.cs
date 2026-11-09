using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance { get; private set; }
    private int currentSaveDataIndex;
    private SaveData currentData;
    // 本次运行的游玩计时（秒），只在实际游玩时累加
    private float sessionSeconds;
    private bool isBusy = false;
    public static Action OnSaveSuccess;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        GameManager.Instance.OnLoadCompleted += RestorePlayerData;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLoadCompleted -= RestorePlayerData;
    }

    private string GetSaveFolder()
    {
        string folder = Path.Combine(Application.persistentDataPath, "SaveData");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return folder;
    }

    private string GetSavePath(int index)
    {
        return Path.Combine(GetSaveFolder(), $"save_{index}.json");
    }

    public string GetScreenshotPath(int index)
    {
        return Path.Combine(GetSaveFolder(), $"save_{index}.png");
    }


    public bool HasAnySave()
    {
        string folder = GetSaveFolder();
        string[] files = Directory.GetFiles(folder, "save_*.json");
        return files.Length > 0;
    } 
    public int HasAnySaveNum()
    {
        string folder = GetSaveFolder();
        string[] files = Directory.GetFiles(folder, "save_*.json");
        return files.Length;
    }

    private void Update()
    {
        // 只在实际游玩时计时：场景里有玩家、且没有暂停（timeScale > 0）
        if (Time.timeScale > 0f && PlayerManager.Player != null)
        {
            sessionSeconds += Time.unscaledDeltaTime;
        }
    }

    /// <summary>读某个槽位的存档数据，不改动 currentData（给存档列表 UI 用）</summary>
    public SaveData PeekSaveData(int index)
    {
        string path = GetSavePath(index);
        if (!File.Exists(path))
        {
            string bak = path + ".bak";
            if (File.Exists(bak)) path = bak;
            else return null;
        }
        try { return JsonUtility.FromJson<SaveData>(File.ReadAllText(path)); }
        catch { return null; }
    }

    public void CreateNewData()
    {
        sessionSeconds = 0f;
        SaveData newdata = new SaveData();
        int n = PlayerPrefs.GetInt("DataNum");
        newdata.index = ++n;
        PlayerPrefs.SetInt("DataNum", n);
        PlayerPrefs.Save();
        currentSaveDataIndex = n;
        currentData = newdata;
    }

    public bool GetCurrentSaveData(int index)
    {
        if (index == 0) index = PlayerPrefs.GetInt("LastPlaySave");
        string path = GetSavePath(index);

        if (!File.Exists(path))
        {
            string backupPath = path + ".bak";
            if (File.Exists(backupPath))
            {
                Debug.LogWarning("检测到存档缺失，从备份恢复");
                File.Copy(backupPath, path);
            }
            else
            {
                Debug.LogWarning($"存档不存在：槽位 {index}");
                return false;
            }
        }

        try
        {
            string json = File.ReadAllText(path);
            currentData = JsonUtility.FromJson<SaveData>(json);

            if (currentData == null)
            {
                Debug.LogError("存档解析失败");
                return false;
            }

            currentSaveDataIndex = index;
            sessionSeconds = currentData.playSeconds;   // 读档后接着上一次的时长继续算
            Debug.Log($"加载存档数据成功：槽位 {index}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"读取失败：{e.Message}");
            return false;
        }
    }


    public void Load(int index)
    {
        if (isBusy)
        {
            Debug.LogWarning("正在处理中，请稍后再试");
            return;
        }
        StartCoroutine(LoadCoroutine(index));
    }
     public void LoadLastSaveData()
    {
        if (isBusy)
        {
            Debug.LogWarning("正在处理中，请稍后再试");
            return;
        }
        StartCoroutine(LoadCoroutine(currentSaveDataIndex));
    }

    private IEnumerator LoadCoroutine(int index)
    {
        isBusy = true;
        bool success = GetCurrentSaveData(index);
        yield return null;

        if (!success)
        {
            isBusy = false;
            yield break;
        }

        RestoreScene();
        isBusy = false;
        Debug.Log($"读档完成：槽位 {index}");
    }

 

    public void Save()
    {
        if (isBusy)
        {
            Debug.LogWarning("正在保存中");
            return;
        }
        StartCoroutine(SaveCoroutine());
        PlayerPrefs.SetInt("LastPlaySave", currentSaveDataIndex);
        PlayerPrefs.Save();
    }

    private IEnumerator SaveCoroutine()
    {
        isBusy = true;
        yield return new WaitForEndOfFrame();

        var player = PlayerManager.Player;
        if (currentData != null && player != null)
        {
            currentData.currentHealth = player.CurrentHealth;
            currentData.currentPosture = player.currentPosture;
            currentData.PlayerPosition = player.transform.position;
            currentData.PlayerRotation = player.transform.eulerAngles;
            currentData.isNew = false;
        }

        // 写入游玩时长与存档时间
        if (currentData != null)
        {
            currentData.playSeconds = sessionSeconds;
            currentData.saveTimeIso = System.DateTime.Now.ToString("o");
        }

        // ===== 截图并保存为 PNG 文件 =====
        byte[] screenshot = CaptureScreenshot();
        string pngPath = GetScreenshotPath(currentSaveDataIndex);

        // JSON 里不再存截图二进制
        if (currentData != null) currentData.screenshotData = null;

        string json = JsonUtility.ToJson(currentData, true);
        string jsonPath = GetSavePath(currentSaveDataIndex);

        bool done = false;
        Exception error = null;

        Task.Run(() =>
        {
            try
            {
                // 1. 先写 PNG 图片
                if (screenshot != null)
                    File.WriteAllBytes(pngPath, screenshot);

                // 2. 再写 JSON
                WriteSaveFile(jsonPath, json);
            }
            catch (Exception e) { error = e; }
            finally { done = true; }
        });

        while (!done) yield return null;

        if (error != null)
            Debug.LogError($"保存失败：{error.Message}");
        else
        {
            Debug.Log($"存档成功：槽位 {currentSaveDataIndex}");
            OnSaveSuccess?.Invoke();
        }

        isBusy = false;
    }


  
    public byte[] LoadScreenshot(int index)
    {
        string path = GetScreenshotPath(index);
        if (!File.Exists(path)) return null;

        try
        {
            return File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            Debug.LogError($"读取截图失败：{e.Message}");
            return null;
        }
    }


    public Sprite LoadScreenshotAsSprite(int index)
    {
        byte[] bytes = LoadScreenshot(index);
        if (bytes == null) return null;

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
        return sprite;
    }


    public void Delete(int index)
    {
        string jsonPath = GetSavePath(index);
        string pngPath = GetScreenshotPath(index);

        if (File.Exists(jsonPath)) File.Delete(jsonPath);
        if (File.Exists(jsonPath + ".bak")) File.Delete(jsonPath + ".bak");
        if (File.Exists(jsonPath + ".tmp")) File.Delete(jsonPath + ".tmp");
        if (File.Exists(pngPath)) File.Delete(pngPath);

        Debug.Log($"已删除存档：槽位 {index}");
    }

    private void RestorePlayerData()
    {
        if (currentData.isNew) return;//新存档不需要恢复数据
        var player = PlayerManager.Player;
        if (player == null)
        {
            Debug.LogWarning("玩家不存在，无法恢复数据");
            return;
        }

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = currentData.PlayerPosition;
        player.transform.eulerAngles = currentData.PlayerRotation;

        if (cc != null) cc.enabled = true;

        player.SetHealth(currentData.currentHealth);
        player.SetPosture(currentData.currentPosture);
    }

    private void RestoreScene()
    {
        GameManager.Instance.LoadSceneAsync("level_1");
    }


    private byte[] CaptureScreenshot()
    {
        try
        {
            int w = 320, h = 180;

            Texture2D full = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            full.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            full.Apply();

            Texture2D small = new Texture2D(w, h, TextureFormat.RGB24, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    small.SetPixel(x, y, full.GetPixelBilinear((float)x / w, (float)y / h));
            small.Apply();

            byte[] bytes = small.EncodeToPNG();

            Destroy(full);
            Destroy(small);

            return bytes;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"截图失败：{e.Message}");
            return null;
        }
    }

  
    private void WriteSaveFile(string path, string json)
    {
        string tempPath = path + ".tmp";
        string backupPath = path + ".bak";

        File.WriteAllText(tempPath, json);

        if (File.Exists(path))
        {
            if (File.Exists(backupPath)) File.Delete(backupPath);
            File.Move(path, backupPath);
        }

        File.Move(tempPath, path);
    }
}