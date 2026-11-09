using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSTLarp : STLarp
{
    protected  void Start()
    {
        PlayerManager.Player.OnPostureChanged+= OnPosture;
    }
}
