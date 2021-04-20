using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementController))]
public class PlayerReload : MonoBehaviour
{
    MovementController controller;

    void Awake()
    {
        controller = GetComponent<MovementController>();
    }

    void Start()
    {
        Vector3 position;
        Quaternion rotation;
        GameManager.instance.GetCurrentCheckpoint(out position, out rotation);
        MoveTo(position);
    }

    void MoveTo(Vector3 position)
    {
        controller.collisionEnabled = false;
        controller.groundSnap = false;
        controller.resolveGroundPenetration = false;
        controller.gravityEnabled = false;

        // controller.MoveTo(position);
        // controller.Tick(Time.deltaTime);
        controller.Position = position;
        transform.position = position;
    }
}
