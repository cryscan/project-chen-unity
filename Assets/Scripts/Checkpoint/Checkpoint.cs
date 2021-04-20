using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int index;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            Debug.Log($"Checkpoint {index} reached");
            GameManager.instance.SetCurrentCheckpoint(index);
        }
    }
}
