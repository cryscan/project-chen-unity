using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    Transform[] checkpoints;

    void Awake()
    {
        checkpoints = Enumerable.Range(0, transform.childCount).Select(x => transform.GetChild(x).transform).ToArray();
    }

    void Start()
    {
        GameManager.instance.SetCheckpoints(checkpoints);
    }
}
