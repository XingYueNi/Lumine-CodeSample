using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCanvans : MonoBehaviour
{
    //主画布管理他之下的所有子界面
    private static MainCanvans _instance;
    public static MainCanvans Instance
    {
        get
        {
            if (_instance == null)
            {
                // 没有 Canvans 的场景（比如纯菜单场景）要允许返回 null，
                // 原写法 Find 返回 null 后直接 GetComponent 会空引用
                GameObject go = GameObject.Find("Canvans");
                if (go != null) _instance = go.GetComponent<MainCanvans>();
            }
            return _instance;
        }
    }
    public GameObject mainCanvans;
    public GameObject gameManagerUI;
    public GameObject pauseMenu;
    public GameObject packageBag;
    public Canvas contextMenu;
    public bool cursorFlag = false;
    public Canvas DeathUI;
    public PlayerManager player;
    public Camera mainCamera;
    private void Awake()
    {
        CurorHideOrShow();
    }
    private void Start()
    {
        UiManager.Instance.OpenPanel("HDDUI");
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            PackageBag();
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            OpenUi();
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            if ((packageBag != null && packageBag.activeSelf) || (pauseMenu != null && pauseMenu.activeSelf))
                return;
            CloseUi();
        }
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
          
        }
        if (contextMenu==null)
        {
            return;
        }
     
    }
    public void PackageBag()//打开或关闭背包
    {
        if (pauseMenu != null)//主菜单打开的情况下不允许打开背包
        {
            return;
        }
        if (packageBag == null)
        {
            packageBag = UiManager.Instance.OpenPanel("PackageBag");//打开
            OpenUi();
        }
        else
        {
            UiManager.Instance.ClosePanel("PackageBag");
            CloseUi();
        }

    }
    public void CurorHideOrShow()//隐藏或显示鼠标光标
    {

        if (!cursorFlag)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        Cursor.visible = cursorFlag;
    }
    public void PauseMenu()//打开或关闭暂停菜单
    {
        //Debug.Log("打开或关闭主菜单");
        if (packageBag != null)
        {
            PackageBag();
            return;
        }//如果背包处于打开状态，则关闭背包
        if (pauseMenu == null)
        {
            pauseMenu = UiManager.Instance.OpenPanel("PauseMenu");//第一次打开
            OpenUi();
        }
        else
        {
            UiManager.Instance.ClosePanel("PauseMenu");
            CloseUi();
        }
    }
    public void OpenUi()//打开UI之后，禁用玩家，时停和显示鼠标指针
    {
        cursorFlag = true;//显示鼠标指针
        player.enabled = false;
        if (mainCamera != null) mainCamera.GetComponent<CinemachineBrain>().enabled = false;
        Time.timeScale = 0;
        CurorHideOrShow();
    } 
    public void CloseUi()//打开UI之后，禁用玩家，时停和显示鼠标指针
    {
        cursorFlag = false;
        player.enabled = true;
        if (mainCamera != null) mainCamera.GetComponent<CinemachineBrain>().enabled = true;
        Time.timeScale = 1;
        CurorHideOrShow();
    }
}
