using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHPLarp : HPImageLarp
{
    protected  void Start()
    {
        PlayerManager.Player.OnHealthChanged += OnHpChange;
    }
}
