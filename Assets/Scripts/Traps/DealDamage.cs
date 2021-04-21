using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        var proxy = collider.GetComponent<PlayerHealthProxy>();
        if (proxy != null)
        {
            proxy.playerHealth.Kill();
        }
    }
}
