using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUI : MonoBehaviour
{
    private Transform PlayerCamera;
    private void Awake()
    {
        GameObject cam = GameObject.Find("MainCamera");
        if (cam != null) PlayerCamera = cam.transform;   
    }
    private void Update()
    {
       if (PlayerCamera != null) transform.LookAt(PlayerCamera);
    }
}
