using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    [SerializeField] LayerMask layer;
    [SerializeField] Hopper hopper;
    [SerializeField] Transform body;
    [SerializeField] Transform target;

    Camera _camera;
    bool locked = false;

    void Awake()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        if (!locked)
        {
            var forward = transform.position - body.position;
            forward.y = 0;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 20, layer))
        {
            var point = hit.point;
            if (!locked) transform.position = point;
            else transform.LookAt(point, Vector3.up);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (locked)
            {
                var position = transform.position;
                position.y = target.position.y;
                target.position = position;
                target.rotation = transform.rotation;

                hopper.Optimize();
            }

            locked = !locked;
        }
    }
}
