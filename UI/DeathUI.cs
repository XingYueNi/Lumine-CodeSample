using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathUI : MonoBehaviour
{
    private Button ResetBt;
    private void Start()
    {
        GameObject resetGo = GameObject.Find("ResetGame");
        if (resetGo != null)
        {
            ResetBt = resetGo.GetComponent<Button>();
            if (ResetBt != null) ResetBt.onClick.AddListener(OnResetBtClick);
        }
        else
        {
            Debug.LogWarning("[DeathUI] 场景里找不到 ResetGame 物体，重开按钮不可用");
        }
    }
    private void OnResetBtClick()
    {
        Debug.Log("游戏已重启");
        SaveDataManager.Instance.LoadLastSaveData();
    }
}
