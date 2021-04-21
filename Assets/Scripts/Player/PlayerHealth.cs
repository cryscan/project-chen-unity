using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AbilityRunner))]
[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(Collider))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] GameObject view;
    [SerializeField] GameObject ragdoll;

    [SerializeField] float killHeight = -1;
    [SerializeField] float reloadTime = 5;

    AbilityRunner abilityRunner;
    MovementController controller;
    Collider _collider;

    public bool alive { get; private set; } = true;

    void Awake()
    {
        abilityRunner = GetComponent<AbilityRunner>();
        controller = GetComponent<MovementController>();
        _collider = GetComponent<Collider>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            Kill();

        if (transform.position.y < killHeight)
            Kill();
    }

    public void Kill()
    {
        if (!alive) return;

        var position = controller.Position;

        ref var synthesizer = ref abilityRunner.Synthesizer.Ref;
        var velocity = synthesizer.CurrentVelocity;

        view.SetActive(false);

        abilityRunner.enabled = false;
        controller.enabled = false;
        //  _collider.enabled = false;

        alive = false;

        var deadBody = Instantiate(ragdoll, position, transform.rotation);

        /*
        var rigidbodies = deadBody.GetComponents<Rigidbody>();
        foreach (var rigidbody in rigidbodies)
            rigidbody.velocity = velocity;
        */

        StartCoroutine(KillCoroutine());
    }

    IEnumerator KillCoroutine()
    {
        yield return new WaitForSeconds(reloadTime);
        GameManager.instance.Reload();
    }
}
