using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageManager : MonoBehaviour
{
    public static PackageManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        if (Instance == null) Instance = this;
        DontDestroyOnLoad(this);

    }
    private PackageTable _packageTable;
    public PackageTable GetPackageTable()
    {
        if (_packageTable == null)
        {
            _packageTable = Resources.Load<PackageTable>("LocalData/PackageTable");
        }
        return _packageTable;
    }
    public List<PackageLocalItem> GetPackageLocal()
    {
        return PackageLocalData.Instance.LoadPackage();
    }
    public PackageTableItem GetPackageItemById(int id)//根据传入的id去本地数据中查找对应的模板
    {
        List<PackageTableItem> packageDataList = GetPackageTable().DataList;
        foreach (PackageTableItem item in packageDataList)
        {
            if (item.id == id)
                return item;
        }
        return null;
    }
    public PackageLocalItem GetPackageLocalItemById(string uid)
    {
        List<PackageLocalItem> packageLocalItems = GetPackageLocal();
        foreach (PackageLocalItem item in packageLocalItems)
        {
            if (item.uid == uid)
                return item;
        }
        return null;
    }
    public class PackageItemComparer : IComparer<PackageLocalItem>
    {
        public int Compare(PackageLocalItem a, PackageLocalItem b)
        {
            PackageTableItem x =PackageManager.Instance. GetPackageItemById(a.id);
            PackageTableItem y = PackageManager.Instance.GetPackageItemById(b.id);
            //  if (x != null && y != null) Debug.Log("已找到对应的模板，x星级为" + x.star + "，y的星级为" + y.star);
            //    else if (x == null || y == null) {Debug.Log("有对象为空"); return 0; }
            int starCompare = y.star.CompareTo(x.star);

            if (starCompare == 0)//xy星级相等
            {
                //     Debug.Log("星级一致");
                int idCompare = y.id.CompareTo(x.id);
                if (idCompare == 0)
                    return b.level.CompareTo(a.level);
                return idCompare;
            }

            //  Debug.Log("星级不一致");
            return starCompare;
        }
    }
    public List<PackageLocalItem> GetSortPackageLocalData()
    {
        List<PackageLocalItem> localItems = PackageLocalData.Instance.LoadPackage();
        localItems.Sort(new PackageItemComparer());//需要实现Icompare接口，内部使用快速排序和插入排序的混合算法   
        return localItems;//返回排序好的数据
    }
}
