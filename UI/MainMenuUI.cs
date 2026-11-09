using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public GameObject ContinueText;
    public GameObject NewGameText;
    public GameObject LoadGameText;
    public GameObject QuitGameText;
    private void Start()
    {
        initUI();
    }
    private void OnDisable() { 
        ContinueText.GetComponent<Button>().onClick.RemoveListener(ContinueGame);
        NewGameText.GetComponent<Button>().onClick.RemoveListener(CreateNewGame);
        LoadGameText.GetComponent<Button>().onClick.RemoveListener(OpenLoadGameUI);
        QuitGameText.GetComponent<Button>().onClick.RemoveListener(GameManager.Instance.ExitGame);
    }
    private void initUI()
    {

        ContinueText = transform.Find("MainUI/Continue").gameObject;
        NewGameText = transform.Find("MainUI/NewGame").gameObject;
        LoadGameText = transform.Find("MainUI/LoadGame").gameObject;
        QuitGameText = transform.Find("MainUI/QuitGame").gameObject;
        ContinueText.GetComponent<Button>().onClick.AddListener(ContinueGame);
        NewGameText.GetComponent<Button>().onClick.AddListener(CreateNewGame);
        LoadGameText.GetComponent<Button>().onClick.AddListener(OpenLoadGameUI);
        QuitGameText.GetComponent<Button>().onClick.AddListener(GameManager.Instance.ExitGame);
        if (SaveDataManager.Instance == null)
        {
            Debug.LogError("检查场景里有没有挂 SaveDataManager");
            return;
        }
        if (!SaveDataManager.Instance.HasAnySave())
        {
            //   Debug.Log("当前无可用存档");
            ContinueText.GetComponent<TextMeshProUGUI>().color = new Color(0.18f, 0.18f, 0.18f);
            ContinueText.GetComponent<Button>().enabled = false;
        }
        else
            ContinueText.GetComponent<Button>().enabled = true;
    }
    private void ContinueGame()
    {
        Debug.Log("继续上次游戏");
        SaveDataManager.Instance.Load(0);
    }
    private void CreateNewGame()
    {
        Debug.Log("创建新游戏存档");
        SaveDataManager.Instance.CreateNewData();
        GameManager.Instance.LoadSceneAsync("level_1");
    }
    private void OpenLoadGameUI()
    {
        transform.Find("LoadGameUI").gameObject.SetActive(true);
    }
}
