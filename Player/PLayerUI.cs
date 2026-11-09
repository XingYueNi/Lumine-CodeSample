using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PLayerUI : MonoBehaviour
{
    public GameObject DeathUI;
    private void OnEnable()
    {
        PlayerManager.Player.OnDeath += OpenUI;
    }
    public void OpenUI()
    {
        DeathUI.gameObject.SetActive(true);
    }
    public void CloseUI()
    {
        DeathUI.gameObject.SetActive(false);
    }
}
