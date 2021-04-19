using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AbilityRunner))]
[RequireComponent(typeof(Animator))]
public class PlayerReload : MonoBehaviour
{
    AbilityRunner abilityRunner;
    Animator animator;

    void Awake()
    {
        abilityRunner = GetComponent<AbilityRunner>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            var checkpoint = GameManager.instance.GetCurrentCheckpoint();
            abilityRunner.MoveTo(checkpoint.t, checkpoint.q);
        }
    }
}
