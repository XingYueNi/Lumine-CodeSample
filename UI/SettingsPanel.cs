using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsPanel : BasePanel
{
    [Header("画质档位")]
    [SerializeField] Dropdown qualityDropdown;
    [SerializeField] Image qualityBackground;

    [Header("全屏/窗口")]
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] TextMeshProUGUI fullscreenLabel;

    [Header("音频")]
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] TextMeshProUGUI masterVolumeLabel;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] TextMeshProUGUI musicVolumeLabel;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] TextMeshProUGUI sfxVolumeLabel;
    [SerializeField] Toggle muteToggle;

    [Header("关闭")]
    [SerializeField] Button closeButton;

    private const string PanelName = "SettingsPanel";

    /// <summary>音量由场景中的 AudioVolumeController 统一驱动（AudioMixer 分组）</summary>
    private AudioVolumeController Audio
    {
        get { return AudioVolumeController.Instance; }
    }

    private void OnEnable()
    {
        InitializeQualityDropdown();
        InitializeFullscreenToggle();
        InitializeAudio();

        if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (closeButton != null) closeButton.onClick.AddListener(CloseSelf);
    }

    private void Start()
    {
        ApplySavedSettings();
    }

    private void OnDisable()
    {
        // 面板关闭时会销毁，这里只是防止重复进入时监听叠加
        if (qualityDropdown != null) qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        if (closeButton != null) closeButton.onClick.RemoveListener(CloseSelf);

        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterChanged);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxChanged);
        if (muteToggle != null) muteToggle.onValueChanged.RemoveListener(OnMuteChanged);
    }

    private void CloseSelf()
    {
        if (UiManager.Instance != null) UiManager.Instance.ClosePanel(PanelName);
    }

    // =====================================================================
    //  画质
    // =====================================================================
    private void InitializeQualityDropdown()
    {
        if (qualityDropdown == null) return;

        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        options.Add(new Dropdown.OptionData("非常低"));
        options.Add(new Dropdown.OptionData("中等"));
        options.Add(new Dropdown.OptionData("非常高"));

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(options);
    }

    private void OnQualityChanged(int optionIndex)
    {
        // 下拉只有 3 项，而 QualitySettings 可能有 6 档，需要按比例映射
        int levelCount = QualitySettings.names.Length;
        if (levelCount <= 0) return;

        int target = Mathf.RoundToInt(optionIndex * (levelCount - 1) / 2f);
        target = Mathf.Clamp(target, 0, levelCount - 1);

        QualitySettings.SetQualityLevel(target, true);
        PlayerPrefs.SetInt("QualityLevel", optionIndex);
        PlayerPrefs.Save();
    }

    // =====================================================================
    //  全屏
    // =====================================================================
    private void InitializeFullscreenToggle()
    {
        if (fullscreenToggle == null) return;

        bool isFull = Screen.fullScreen;
        fullscreenToggle.SetIsOnWithoutNotify(isFull);
        if (fullscreenLabel != null) fullscreenLabel.text = "全屏：" + (isFull ? "开" : "关");
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        if (fullscreenLabel != null) fullscreenLabel.text = "全屏：" + (isFullscreen ? "开" : "关");
    }

    // =====================================================================
    //  音频
    // =====================================================================
    private void InitializeAudio()
    {
        if (masterVolumeSlider == null || musicVolumeSlider == null || sfxVolumeSlider == null) return;

        masterVolumeSlider.minValue = 0f;
        masterVolumeSlider.maxValue = 1f;
        musicVolumeSlider.minValue = 0f;
        musicVolumeSlider.maxValue = 1f;
        sfxVolumeSlider.minValue = 0f;
        sfxVolumeSlider.maxValue = 1f;

        AudioVolumeController av = Audio;
        if (av != null)
        {
            // 用 SetValueWithoutNotify，避免回填时触发回调把值又写回混音器
            masterVolumeSlider.SetValueWithoutNotify(av.Master);
            musicVolumeSlider.SetValueWithoutNotify(av.Music);
            sfxVolumeSlider.SetValueWithoutNotify(av.Sfx);
            if (muteToggle != null) muteToggle.SetIsOnWithoutNotify(av.Muted);
        }
        else
        {
            Debug.LogWarning("[设置] 场景中没有 AudioVolumeController，这次改动不会真的改音量。");
        }

        // 监听器无论如何都要绑：否则滑条拖得动、标签却不跟着变
        masterVolumeSlider.onValueChanged.AddListener(OnMasterChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxChanged);
        if (muteToggle != null) muteToggle.onValueChanged.AddListener(OnMuteChanged);

        RefreshAudioLabels();
    }

    private void OnMasterChanged(float value)
    {
        if (Audio != null) Audio.SetMaster(value);
        RefreshAudioLabels();
    }

    private void OnMusicChanged(float value)
    {
        if (Audio != null) Audio.SetMusic(value);
        RefreshAudioLabels();
    }

    private void OnSfxChanged(float value)
    {
        if (Audio != null) Audio.SetSfx(value);
        RefreshAudioLabels();
    }

    private void OnMuteChanged(bool muted)
    {
        if (Audio != null) Audio.SetMuted(muted);
        RefreshAudioLabels();
    }

    private void RefreshAudioLabels()
    {
        if (masterVolumeLabel != null && masterVolumeSlider != null)
            masterVolumeLabel.text = "主音量：" + Mathf.RoundToInt(masterVolumeSlider.value * 100f) + "%";

        if (musicVolumeLabel != null && musicVolumeSlider != null)
            musicVolumeLabel.text = "背景音乐：" + Mathf.RoundToInt(musicVolumeSlider.value * 100f) + "%";

        if (sfxVolumeLabel != null && sfxVolumeSlider != null)
            sfxVolumeLabel.text = "音效音量：" + Mathf.RoundToInt(sfxVolumeSlider.value * 100f) + "%";
    }

    // =====================================================================
    //  启动时应用存档
    // =====================================================================
    private void ApplySavedSettings()
    {
        // 画质
        if (qualityDropdown != null && qualityDropdown.options.Count > 0)
        {
            int savedIndex = PlayerPrefs.GetInt("QualityLevel", 0);
            savedIndex = Mathf.Clamp(savedIndex, 0, qualityDropdown.options.Count - 1);
            qualityDropdown.SetValueWithoutNotify(savedIndex);

            int levelCount = QualitySettings.names.Length;
            if (levelCount > 0)
            {
                int target = Mathf.Clamp(
                    Mathf.RoundToInt(savedIndex * (levelCount - 1) / 2f), 0, levelCount - 1);
                QualitySettings.SetQualityLevel(target, true);
            }
        }

        // 全屏
        bool isFull = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = isFull;
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(isFull);
        if (fullscreenLabel != null) fullscreenLabel.text = "全屏：" + (isFull ? "开" : "关");

        // 音频由 AudioVolumeController 在 Awake 里读取 PlayerPrefs 并 Apply，这里无需重复处理
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSelf();
        }
    }
}
