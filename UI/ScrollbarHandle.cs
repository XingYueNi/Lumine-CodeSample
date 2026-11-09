using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollbarHandle : MonoBehaviour
{
    private Scrollbar scrollbar;
    private float speed=0.12f;
    private void Awake()
    {
        scrollbar = gameObject.GetComponent<Scrollbar>();
    }
    private void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            scrollbar.value += speed;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            scrollbar.value -= speed;
        }
    }
}
