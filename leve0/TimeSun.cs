using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TimeSun : MonoBehaviour
{
    float gameTimer;
    public float gameTimerChange;
    public float initialGameTimer;
    public Light sun, moon;
    public TMPro.TMP_Text timeText;
    float h, m;
    public Vector3 anglesx, anglesy;
    public double x, y;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        gameTimer = initialGameTimer;
        GameTimeChange();
    }
    private void Update()
    {
        GameTimeSum();
    }
    private void GameTimeChange()
    {
        gameTimer = gameTimerChange;
        StartCoroutine(GameLingChange());
    }
    private IEnumerator GameLingChange()
    {
        y=x=gameTimer*0.25;
        y += 180;
        anglesx = sun.gameObject.transform.eulerAngles;
        while (Mathf.Floor( anglesx.x)!=x)
        {
            sun.gameObject.transform.Rotate(0.25f, 0, 0);
            moon.gameObject.transform.Rotate(0.25f, 0, 0);
            anglesx = sun.gameObject.transform.eulerAngles;
            anglesy = moon.gameObject.transform.eulerAngles;
         
            yield  return null;
            Debug.Log("anglesx.x="+Mathf.Floor(anglesx.x) + "x="+x+"local"+sun.gameObject.transform.eulerAngles );
        }
        Debug.Log("����Э��");
        yield break;
        
    }

    private void GameTimeSum()
    {
        if (gameTimer >= 1440f)
        {
            gameTimer = 0;
        }
        gameTimer += Time.deltaTime;
        h = Mathf.Floor(gameTimer / 60);//��ȡСʱ
        m = Mathf.Floor(gameTimer % 60);//��ȡ����
        if (h < 10)
        {
            timeText.text = "0" + h.ToString();
        }
        else
        {
            timeText.text = h.ToString();
        }
        timeText.text += ":";
        if (m < 10)
        {
            timeText.text += "0" + m.ToString();
        }
        else
        {
            timeText.text += m.ToString();
        }
        return;
    }
}