using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PackageCell : MonoBehaviour,IPointerClickHandler
{
    private Transform UIIcon;
    private Transform UIHead;
    private Transform UINew;
    private Transform UISelect;
    private Transform UILevel;
    private Transform UIStars;
    private Transform UIDeleteSelect;
    private PackageLocalItem packageLocalData;
    private PackageTableItem packageTableItem;
    private PackacgPanel uiParent;
    private Transform Top;//��ʵ�Ǳ�����ɫ
    private Color color_3= new Color(0f, 0.44f, 0.87f, 1f);
    private Color color_4= new Color(0.64f, 0.21f, 0.93f, 1f);
    private Color color_5=new Color(0.85f, 0.67f, 0.22f, 1f);
    // Start is called before the first frame update
    private void Awake()
    {
        InitUIName();
    }

    private void InitUIName()
    {
        UIIcon = transform.Find("Top/Icon");
        UIHead = transform.Find("Top/Head");
        UINew = transform.Find("Top/NewIcon");
        UILevel = transform.Find("Bottom/LevelText");
        UIStars = transform.Find("Bottom/Stars");
        UISelect = transform.Find("Select");
        Top = transform.Find("Top");
        UIDeleteSelect = transform.Find("DeleteSelect");
        UIDeleteSelect.gameObject.SetActive(false);
    }
    public void Refresh(PackageLocalItem packageLocalData, PackacgPanel uiParent)//��ÿ����������������Ϣˢ��
    {
        this.packageLocalData = packageLocalData;
        this.packageTableItem = PackageManager.Instance.GetPackageItemById(packageLocalData.id);
        this.uiParent = uiParent;
        UILevel.GetComponent<TMPro.TMP_Text>().text = "LV." + this.packageLocalData.level.ToString();
        UINew.gameObject.SetActive(this.packageLocalData.isNew);
        //   Texture2D t = (Texture2D)Resources.Load(this.packageTableItem.imagePath);
        // if (t == null) { Debug.Log("δ��ȡ��ͼƬ,ͼƬ·��Ϊ:" + this.packageTableItem.imagePath); return; }
        //    else Debug.Log("�ɹ���ȡ��ͼƬ" + this.packageTableItem.imagePath);
        // Sprite temp = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0, 0));
        Sprite temp = WeaponIconLoader.Instance.GetIcon(this.packageTableItem.name);
        UIIcon.GetComponent<Image>().sprite = temp;
        ReFreshStar();//ˢ���Ǽ�
    }
    private void ReFreshStar()
    {
    //    Debug.Log("��ǰ�����Ǽ�Ϊ" + UIStars.childCount);
        for (int i = 0; i < UIStars.childCount; i++)
        {
            Transform star = UIStars.GetChild(i);
            if (this.packageTableItem.star > i)
            {
                star.gameObject.SetActive(true);
            }
            else
                star.gameObject.SetActive(false);
        }
        if (this.packageTableItem.star == 3)
            Top.GetComponent<Image>().color = color_3;
        else if(this.packageTableItem.star == 4)
            Top.GetComponent<Image>().color = color_4;
        else
            Top.GetComponent<Image>().color = color_5;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(uiParent.curState==PackacgPanel.PackageState.Nomal)//����ģʽ
        {
            uiParent.CurrentItmeUID(packageLocalData.uid);
            packageLocalData.isNew = false;
            UINew.gameObject.SetActive(this.packageLocalData.isNew);
       }
        else if(uiParent.curState == PackacgPanel.PackageState.Delet)//ɾ��ģʽ
        {
            if(uiParent.deletObject.Contains(packageLocalData.uid))//�ȼ�鵱ǰ�����Ƿ���ɾ���б�
            {
                uiParent.deletObject.Remove(packageLocalData.uid);
                UIDeleteSelect.gameObject.SetActive(false);
                return;
            }
            uiParent.deletObject.Add(packageLocalData.uid);
            UIDeleteSelect.gameObject.SetActive(true);
       }
        
    }
}
