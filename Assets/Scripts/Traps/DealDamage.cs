using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        var hitBox = collider.GetComponent<PlayerHitBox>();
        if (hitBox != null)
            hitBox.playerHealth.Kill();
    }
}
