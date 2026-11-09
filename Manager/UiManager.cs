using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManager 
{
    private static UiManager _instance;
    private Transform _uiRoot;
    private Dictionary<string, string> pathDict;//路径配置
    private Dictionary<string, GameObject> prefabDict;//预制体缓存
    private Dictionary<string, BasePanel> panelDict;//已经打开的界面记录
    public static UiManager Instance
    {
        get { 
            if (_instance == null) 
                _instance = new UiManager();
            return _instance;
        }
    }
    public Transform UiRoot
    {
        get
        {
            if (_uiRoot == null)
            {
                GameObject go = GameObject.Find("Canvans");
                if (go != null)
                {
                    _uiRoot = go.transform;
                }
                else
                {
                    // Level_0（主菜单场景）里没有 Canvans，退化为取场景里的第一个 Canvas
                    Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
                    if (canvas != null) _uiRoot = canvas.transform;
                }
            }
            return _uiRoot;
        }

    }
    public class UiTitleConst
    {
        public const string PauseMenu = "PauseMenu";
        public const string HDDUI = "HDDUI";
        public const string GameManagerUI = "GameManagerUI";
        public const string PackageBag = "PackageBag";
        public const string SettingsPanel = "SettingsPanel";
    }
    private void InitDicts()
    {
        prefabDict = new Dictionary<string, GameObject>();
        panelDict = new Dictionary<string, BasePanel>();
        pathDict = new Dictionary<string, string>()
        {
            {UiTitleConst.PauseMenu,"UI/PauseMenu" },
            {UiTitleConst.HDDUI,"UI/HDDUI" },
            {UiTitleConst.GameManagerUI,"UI/GameManagerUI" },
            {UiTitleConst.PackageBag,"UI/PackageBag" },
            {UiTitleConst.SettingsPanel,"UI/SettingsPanel" }
        };
    }
    private UiManager()
    {
        InitDicts();
    }
    public GameObject OpenPanel(string name)//传入界面名称
    {
        BasePanel panel = null;
        if(panelDict.TryGetValue(name,out panel))//先检查是否打开过
        {
           Debug.LogError("界面"+panel + "已经打开过了");
            return null;
        }
        string path = null;
        if(!pathDict.TryGetValue(name,out path))//根据传入的名称查找路径
        {
            Debug.LogError("该界面" + name + "未配置/查找不到路径");
            return null;
        }
        GameObject panelPrefab = null;
        if(!prefabDict.TryGetValue(name,out panelPrefab))//从预制体缓存中查找,如果没找到
        {
            panelPrefab = Resources.Load<GameObject>(path);
            prefabDict.Add(name, panelPrefab);//把找到的添加进缓存
         //Debug.Log("已将预制体" + name + "添加进缓存");
        }
        GameObject panelObject = GameObject.Instantiate(panelPrefab, UiRoot,false);
        panel = panelObject.GetComponent<BasePanel>();
        panelDict.Add(name, panel);
        //Debug.Log("已打开界面" + name);
        return panelObject;
    }
    public bool ClosePanel(string name)
    {
        BasePanel panel = null;
        if(!panelDict.TryGetValue(name,out panel))
        {
            Debug.LogError(name + "该界面未打开");
            return false;
        }
        panel.ClosePanel();
        panelDict.Remove(name);
        return true;
    }

    
}
