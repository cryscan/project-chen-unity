using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Cinemachine;

public class Checkpoint : MonoBehaviour
{
    public AudioMixerSnapshot audioMixerSnapshot;
    public CinemachineVirtualCamera virtualCamera;

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
