using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 全局音量控制（音量 / 音乐 / 音效 / 静音）。
///
/// 设计要点：
/// 1) 用 AudioMixer 的分组来控制，而不是逐个改 AudioSource.volume。
///    只要把每个 AudioSource 的 Output 拖到对应分组，代码就完全不用碰它们。
/// 2) 混音器的音量单位是 dB，不是 0~1，必须用 Mathf.Log10 换算。
/// 3) 本组件不做 DontDestroyOnLoad —— 它通常挂在 Manager 这类场景物体上，
///    整体搬走会把 GameManager 等一起带到下一个场景。混音器本身是工程资产，
///    数值在应用运行期间一直有效，每个场景启动时重新 Apply 一次即可。
/// </summary>
public class AudioVolumeController : MonoBehaviour
{
    public static AudioVolumeController Instance { get; private set; }

    [Header("混音器（GameAudio）")]
    public AudioMixer mixer;

    [Header("暴露参数名（必须与 Exposed Parameters 完全一致）")]
    public string masterParam = "MasterVolume";
    public string musicParam = "MusicVolume";
    public string sfxParam = "SFXVolume";

    private const string KeyMaster = "MasterVolume";
    private const string KeyMusic = "MusicVolume";
    private const string KeySfx = "SfxVolume";
    private const string KeyMute = "MuteToggle";

    /// <summary>主音量，0~1</summary>
    public float Master { get; private set; }
    /// <summary>音乐音量，0~1</summary>
    public float Music { get; private set; }
    /// <summary>音效音量，0~1</summary>
    public float Sfx { get; private set; }
    /// <summary>是否静音</summary>
    public bool Muted { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[音频] 场景中存在多个 AudioVolumeController，已忽略：" + name);
            enabled = false;
            return;
        }
        Instance = this;

        Master = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyMaster, 1f));
        Music = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyMusic, 0.8f));
        Sfx = Mathf.Clamp01(PlayerPrefs.GetFloat(KeySfx, 1f));
        Muted = PlayerPrefs.GetInt(KeyMute, 0) == 1;

        Apply();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------------- 对外接口 ----------------

    public void SetMaster(float value)
    {
        Master = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeyMaster, Master);
        Apply();
    }

    public void SetMusic(float value)
    {
        Music = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeyMusic, Music);
        Apply();
    }

    public void SetSfx(float value)
    {
        Sfx = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeySfx, Sfx);
        Apply();
    }

    public void SetMuted(bool muted)
    {
        Muted = muted;
        PlayerPrefs.SetInt(KeyMute, muted ? 1 : 0);
        Apply();
    }

    /// <summary>把当前保存的值写进混音器</summary>
    public void Apply()
    {
        // 静音只压总音量，音乐与音效各自的滑杆值保留，取消静音后原样恢复
        SetDb(masterParam, Muted ? 0f : Master);
        SetDb(musicParam, Music);
        SetDb(sfxParam, Sfx);

        PlayerPrefs.Save();
    }

    // ---------------- 内部实现 ----------------

    private void SetDb(string param, float linear)
    {
        if (mixer == null)
        {
            Debug.LogWarning("[音频] AudioVolumeController 没有指定 mixer，音量控制无效。");
            return;
        }
        if (string.IsNullOrEmpty(param)) return;

        // 线性 0~1 换算成 dB：0 -> -80dB（静音），1 -> 0dB，0.5 -> 约 -6dB
        float db = linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
        mixer.SetFloat(param, db);
    }
}
