using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class STLarp : MonoBehaviour
{
    public Image image_left;
    public Image image_right;
    protected Color color_1, color_2, color_3;
    protected virtual void OnEnable()
    {
        color_1 = new Color(1f, 0.8f, 0.65f, 1);
        color_2 = new Color(0.96f, 0.34f, 0.09f, 1);
        color_3 = new Color(0.85f, 0.05f, 0, 1);
    }
    public void OnPosture(float newPosture)
    {
        if (newPosture > 80)
        {
            image_left.color = color_3;
            image_right.color = color_3;
        }else if (newPosture > 40)
        {
            image_left.color = color_2;
            image_right.color = color_2;
        }
        else
        {
            image_left.color = color_1;
            image_right.color = color_1;
        }
        image_right.fillAmount = image_left.fillAmount = newPosture / 100;
    }
}
