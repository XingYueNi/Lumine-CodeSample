using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private Button ContinueBt;
    private Button SettingBt;
    private Button SaveAndExitBt;
    private Button BackMainMenuBt;
    private void Start()
    {
        InitUI();
    }
    private void InitUI()
    {
        ContinueBt = transform.Find("Image/ContinueBt").GetComponent<Button>();
        SettingBt = transform.Find("Image/SettingBt").GetComponent<Button>();
        SaveAndExitBt = transform.Find("Image/SaveAndExitBt").GetComponent<Button>();
        BackMainMenuBt = transform.Find("Image/BackMainMenuBt").GetComponent<Button>();
        ContinueBt.onClick.AddListener(MainCanvans.Instance.PauseMenu);
        SaveAndExitBt.onClick.AddListener(SaveAndExit);
        SettingBt.onClick.AddListener(OpenSetting);
        BackMainMenuBt.onClick.AddListener(BackMainMenu);
      
    }
    private void OnDisable()
    {
        SaveAndExitBt.onClick.RemoveListener(SaveAndExit);
        BackMainMenuBt.onClick.RemoveListener(BackMainMenu);
    }
    private void SaveAndExit()
    {
        Debug.Log("保存并退出");

        if(SaveDataManager.Instance!=null)
        SaveDataManager.Instance.Save();
        else
        {
            Debug.Log("未找到存档管理器");
        }
        UiManager.Instance.ClosePanel("PauseMenu");
    }
    private void BackMainMenu()
    {
        // 关键：先关掉已打开的面板，否则 UiManager 的字典里会残留"已打开"记录，
        // 回到主菜单后再点开始游戏，OpenPanel 会报"已经打开过了"。
        if (UiManager.Instance != null)
        {
            UiManager.Instance.ClosePanel("PauseMenu");
            UiManager.Instance.ClosePanel("HDDUI");
        }

        // 暂停时 Time.timeScale 是 0，不恢复的话主菜单是冻住的
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameManager.Instance.LoadScene("level_0");
    }
    private void OpenSetting()
    {
        UiManager.Instance.ClosePanel("PauseMenu");
        Debug.Log("打开设置面板");
        UiManager.Instance.OpenPanel("SettingsPanel");
    }
}
