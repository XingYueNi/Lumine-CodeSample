using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerTakeDamage : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
           // Debug.Log("√¸÷–µ–»À");
            PlayerManager.Player.OnAttack(other.GetComponent<Enemy>());
        }
    }
   
}
