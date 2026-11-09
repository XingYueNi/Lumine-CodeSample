using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageLocalData
{ 
    private static PackageLocalData _instance;
    public static PackageLocalData Instance 
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PackageLocalData();
            }
            return _instance;
        }
    }
    public List<PackageLocalItem> items;//背包里的所有物品
    public void SavePackacg()
    {
        string inventoryJson = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("PackageLocalData", inventoryJson);
        PlayerPrefs.Save();
    }
    

    public List<PackageLocalItem> LoadPackage()
    {
        if (items != null)
        {
            return items;
        }
        if (PlayerPrefs.HasKey("PackageLocalData"))
        {
            string inventoryJson = PlayerPrefs.GetString("PackageLocalData");
            PackageLocalData packageLocalData = JsonUtility.FromJson<PackageLocalData>(inventoryJson);
            items = packageLocalData.items;
            return items;
        }
        else
        {
            return null;
        }
    }
    public void DeletePackageData()
    {
        PlayerPrefs.DeleteKey("PackageLocalData");
        PlayerPrefs.Save();
    }
   
}
[System.Serializable]
public class PackageLocalItem//背包里单独的每格物品（MC物品格）
{
    public string uid;//唯一标识符
    public int id;
    public int num;//拥有数量
    public int level;//等级
    public bool isNew;//是否第一次获得
    public override string ToString()
    {
        return string.Format("[id]:{0} [num]:{1}[level]:{2}", id, num,level);
    }
}