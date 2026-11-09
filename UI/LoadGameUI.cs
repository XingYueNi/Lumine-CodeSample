using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadGameUI : MonoBehaviour
{
    private int dataNums;
    public TextMeshProUGUI Tip;
    public GameObject saveDataCellPre;
    public Transform uiInstantiateTransform;
    private void Start()
    {
        dataNums=SaveDataManager.Instance.HasAnySaveNum();
        Init();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
    }
    private void Init()
    {
        if (dataNums == 0)
        {
            Tip.gameObject.SetActive(true);
            return;
        }
       for (int i = 1; i <= dataNums; i++)
        {
          Debug.Log("生成第" + i + "个存档槽");
          Instantiate(saveDataCellPre, uiInstantiateTransform, false).GetComponent<SaveDataCell>().Init(i);
        }

    }

}
