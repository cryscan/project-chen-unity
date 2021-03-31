using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ChenRig : MonoBehaviour
{
    [SerializeField] Vector3 velocity;
    [SerializeField] float speed = 1;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        velocity = animator.velocity;
        animator.speed = speed;
    }
}