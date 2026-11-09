using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackageDetail : MonoBehaviour
{
    private Transform UIIcon;
    private Transform UINameText;
    private Transform UILevelText;
    private Transform UIStars;
    private Transform UIDescriptionText;
    private PackageLocalItem packageLocalData;
    private PackageTableItem packageTableItem;
    private PackacgPanel uiParent;
    // Start is called before the first frame update
    private void Awake()
    {
        InitUIName();
    }

    private void InitUIName()
    {
        UIIcon = transform.Find("Image/ItemImage/Image");
        UINameText = transform.Find("Head/NameText");
        UIDescriptionText = transform.Find("Image/LeftImage/descriptionText");
        UIStars = transform.Find("Image/LeftImage/Stars");
        UILevelText = transform.Find("BottomImage/LVBG/LvText");
    }
    public void Refresh(string uid)
    {
        packageLocalData = PackageManager.Instance.GetPackageLocalItemById(uid);
        //if (packageLocalData != null) Debug.Log("����UID��ȡ����Ʒ��Ϣ");
        packageTableItem = PackageManager.Instance.GetPackageItemById(packageLocalData.id);
        UINameText.GetComponent<TMPro.TMP_Text>().text = packageTableItem.name;
        UIDescriptionText.GetComponent<TMPro.TMP_Text>().text = packageTableItem.description;
        UILevelText.GetComponent<TMPro.TMP_Text>().text=packageLocalData.level.ToString();
        // Texture2D t = (Texture2D)Resources.Load(packageTableItem.imagePath);
        //if (t == null) { Debug.Log("δ��ȡ��ͼƬ,ͼƬ·��Ϊ:" + this.packageTableItem.imagePath); return; }
        //  else Debug.Log("�ɹ���ȡ��ͼƬ" + this.packageTableItem.imagePath);
        // Sprite temp = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0, 0));
        Sprite temp = WeaponIconLoader.Instance.GetIcon(this.packageTableItem.name);
        UIIcon.GetComponent<Image>().sprite = temp;
        ReFreshStar();//ˢ���Ǽ�
    }
    private void ReFreshStar()
    {
      //  Debug.Log("��ǰ�����Ǽ�Ϊ" + UIStars.childCount);
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
    }

}   