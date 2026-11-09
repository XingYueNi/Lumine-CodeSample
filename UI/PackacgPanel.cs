using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PackacgPanel : BasePanel
{
    private Transform CloseBt=null;
    private Transform DeletBt;
    private Transform SelectEnterBt;
    private Transform InfoPanel;
    private Transform DeletPanel;
    private Transform UIScrollViewport;
    private Transform ScrollbarVertical;
    private Transform DeleDelEntertBt;
    private GameObject PackageUIItemPrefab=null;
    private PackageDetail packageDetail;
    private string currentItemuid=null;
    public PackageState curState=PackageState.Nomal;
    public List<string> deletObject;
 

    public enum  PackageState
    {
        Nomal,
        Delet
    }
    private void Awake()
    {
        InitUi();
        PackageUIItemPrefab = Resources.Load<GameObject>("UI/PackageContentUIImage");
        deletObject = new List<string>();
    }
    private void Start()
    {
        ReFreshUI();  
    }
    
    public void CurrentItmeUID(string uid)
    {
        currentItemuid = uid;
        packageDetail.Refresh(uid);
    }
    private void ReFreshUI()
    {
        RefreshScroll();
    }
    private void RefreshScroll()
    {
        RectTransform scrollContent = UIScrollViewport.GetComponent<ScrollRect>().content;
        for (int i = 0; i < scrollContent.childCount; i++)
        {
            Destroy(scrollContent.GetChild(i).gameObject);
        }
        foreach (PackageLocalItem item in PackageManager.Instance.GetSortPackageLocalData())
        {
            PackageCell packageCell = Instantiate(PackageUIItemPrefab, scrollContent).GetComponent<PackageCell>();
            packageCell.Refresh(item, this);//将itme的信息更新到新预制体上
            if (currentItemuid == null) CurrentItmeUID(item.uid);//将背包中第一个物体的UID保存下来
        }
    }
    private void InitUi()
    {
        InitUIMenu();
        InitClick(); 
    }
    private void InitUIMenu()
    {
        InfoPanel = transform.Find("Bottom/Info");
        DeletPanel = transform.Find("Bottom/Del");
        CloseBt = transform.Find("Top/RightTop/CloseBt");
        DeletBt = transform.Find("Bottom/Info/leftBottom/DeletBt");//左下角删除键，点击后进入删除页面
        DeleDelEntertBt = transform.Find("Bottom/Del/RightBottom/DelEnterImage");//确认删除键
        SelectEnterBt = transform.Find("Bottom/Del/leftBottom/DeletImage");//左下角返回键
        ScrollbarVertical = transform.Find("Center/LeftCenter/ListView/ScrollbarVertical");
        UIScrollViewport= transform.Find("Center/LeftCenter/ListView");
        packageDetail = transform.Find("Center/PackageDetail").GetComponent<PackageDetail>();
    }
    private void InitClick()
    {
        
        CloseBt.GetComponent<Button>().onClick.AddListener(OnClickClose);
    
        DeletBt.GetComponent<Button>().onClick.AddListener(OnClickDeletPanel);

        SelectEnterBt.GetComponent<Button>().onClick.AddListener(OnClickSelectEnterBt);
        DeleDelEntertBt.GetComponent<Button>().onClick.AddListener(OnClickDeleDelEntertBt);


    }
    private void OnClickDeleDelEntertBt()
    {
        DeleteItem();
        ReFreshUI();
    }
    private void OnClickSelectEnterBt()
    {
        curState = PackageState.Nomal;
        PacgageDelSwitch();
        deletObject.Clear();
        ReFreshUI();
    }

    private void OnClickClose()
    {
        MainCanvans.Instance.PackageBag();
    }

    private void OnClickDeletPanel()
    {
        Debug.Log("进入删除模式");
        curState = PackageState.Delet;
        PacgageDelSwitch();
    }
    private void PacgageDelSwitch()
    {
        DeletPanel.gameObject.SetActive(curState == PackageState.Delet);
        InfoPanel.gameObject.SetActive(!DeletPanel.gameObject.activeSelf);
    }
    public void DeleteItem()
    {
        foreach(string uid in deletObject)
        {
            PackageLocalItem isRemoveItem = null;
            foreach (PackageLocalItem item in PackageLocalData.Instance.items)
            {
                if(uid==item.uid)
                {
                    isRemoveItem = item;
                }
            }
            if (isRemoveItem == null)
            {
             //   Debug.Log("未找到需要删除的物品");
                continue;
            }
            PackageLocalData.Instance.items.Remove(isRemoveItem);
      
        }
        PackageLocalData.Instance.SavePackacg();
    }
}
