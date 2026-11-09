using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyTakeDamage : MonoBehaviour
{
    public Enemy enemy;
 
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            enemy.OnAttackHit(other.gameObject.GetComponent<PlayerManager>(),transform);
        }
    }
   
}
