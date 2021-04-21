using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MoveablePlatformController : MonoBehaviour
{
    [SerializeField] MoveablePlatform[] moveablePlatforms;
    public bool on = false;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            foreach (var moveablePlatform in moveablePlatforms)
                moveablePlatform.on = !moveablePlatform.on;

            on = !on;
            animator.SetBool("On", on);
        }
    }
}
