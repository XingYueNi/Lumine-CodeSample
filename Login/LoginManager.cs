using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public static int userFIG;//�û��ǵ�һ�ν�����Ϸ��
    public GameObject loginUI;
    public GameObject register;
    public TMPro.TMP_InputField loginUsername;
    public TMPro.TMP_InputField loginPassword;
    public TMPro.TMP_InputField registerUsername;
    public TMPro.TMP_InputField registerPassword;
    public TMPro.TMP_InputField confirmPassword;
    public TMPro.TMP_Text feedbackText;
    public TMPro.TMP_Text PrassAnyKey;
    private string username;
    private string password;
    public void RegisterUser()
    {
        username = registerUsername.text;
        password = registerPassword.text;
        string var;
        if(username==null||registerPassword==null)
        {
            feedbackText.text = "�˺Ż����벻��Ϊ��";
            return;
        }
        while ((var=PlayerPrefs.GetString("Username")) != null)
        {
            if (username == var)
            {
                feedbackText.text = "�˺��Ѿ����ڣ�����������";
                return;
            }
        }
        PlayerPrefs.SetString("Username", username);
        PlayerPrefs.SetString("Passwrod", password);
        userFIG = 0;
        PlayerPrefs.SetInt("UserFIG", userFIG);//�����0����û�н������Ϸ
        PlayerPrefs.Save();
    }
    public void Login()
    {
        username = PlayerPrefs.GetString("Username");
        password = PlayerPrefs.GetString("Password");
        userFIG = PlayerPrefs.GetInt("UserFIG");
        #region ����߼�
        //Զ���������ݿ⣬��ѯ��Ӧ���˺��Ƿ����
        //�������ݿ�������˻���UID�Ƿ����
        #endregion 
        if (loginUsername.text == username)
        {
            if (loginPassword.text == password)//��¼�ɹ�
            {
                //���ص�¼UI
                PrassAnyKey.gameObject.SetActive(true);
                InsideGame();
            }
        }
    }
    public IEnumerator InsideGame()
    {
        while (true)
        {
            if (Input.anyKeyDown)
            {
                if(userFIG==0)
                {
                    SceneManager.LoadScene("level_0");
                }
                else
                {
                    SceneManager.LoadScene("level_3");
                }
            }
            yield return null;
        }
       
    }
    public void UISwicth()//�л�UI
    {
        if (loginUI.activeSelf)
        {
            loginUI.SetActive(false);
            register.SetActive(true);
        }
        else
        {
            register.SetActive(false);
            loginUI.SetActive(true);
        }
    }
}
