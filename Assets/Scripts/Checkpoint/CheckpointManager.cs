using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    Checkpoint[] checkpoints;

    void Awake()
    {
        checkpoints = GetComponentsInChildren<Checkpoint>();
        for (int i = 0; i < checkpoints.Length; ++i)
            checkpoints[i].index = i;
    }

    void Start()
    {
        GameManager.instance.ClearCheckpoints();
        foreach (var checkpoint in checkpoints)
            GameManager.instance.AddCheckpoint(checkpoint);
    }
}
