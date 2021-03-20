using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
  * Nominal Configures:
  * Biped:      speed = 2,  steps = 1.5,    gait = Run1.
  * Quadruped:  speed = 1,  steps = 2,      gait = Run2.
  */

public class Pointer : MonoBehaviour
{
    [Header("Configure")]
    [SerializeField] LayerMask layer;
    [SerializeField] Hopper hopper;
    [SerializeField] Transform body;
    [SerializeField] Transform target;
    [SerializeField] GameObject pathPointPrefab;

    [Header("Trajectory")]
    [SerializeField] float speed = 2;
    [SerializeField] float stepsPerSecond = 1.5f;

    [Header("Gaits")]
    [SerializeField] HopperAPI.Gait nominalGait;
    [SerializeField] HopperAPI.Gait endGait;

    Camera _camera;
    bool locked = false;

    List<GameObject> pathPoints = new List<GameObject>();

    void Awake()
    {
        _camera = Camera.main;

        transform.SetParent(null);
        target.SetParent(null);
    }

    void Update()
    {
        UpdateTransform();

        if (Input.GetKeyDown(KeyCode.G))
            hopper.optimizeGaits = !hopper.optimizeGaits;

        if (Input.GetKeyDown(KeyCode.R))
            hopper.timer = 0;

        if (Input.GetMouseButtonDown(0))
        {
            if (locked) AddPathPoint();
            locked = !locked;
        }

        if (Input.GetKeyDown(KeyCode.Return)) Confirm();
    }

    void UpdateTransform()
    {
        if (!locked)
        {
            // var forward = transform.position - body.position;
            // forward.y = 0;
            // transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            transform.rotation = Quaternion.identity;
        }

        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 20, layer))
        {
            var point = hit.point;
            if (!locked) transform.position = point;
            else transform.LookAt(point, Vector3.up);
        }
    }

    void AddPathPoint()
    {
        var pathPoint = Instantiate(pathPointPrefab, transform.position, transform.rotation);
        pathPoints.Add(pathPoint);
    }

    void Confirm()
    {
        locked = false;
        if (pathPoints.Count == 0) return;

        hopper.Reset();

        float distance = Vector3.Distance(body.position, pathPoints[0].transform.position);
        List<float> acc = new List<float>();
        acc.Add(distance);

        for (int i = 0; i < pathPoints.Count - 1; ++i)
        {
            var current = pathPoints[i].transform;
            var next = pathPoints[i + 1].transform;
            distance += Vector3.Distance(current.position, next.position);
            acc.Add(distance);
        }

        float duration = Mathf.Max(distance / speed, 1.2f);
        hopper.duration = duration;

        for (int i = 0; i < pathPoints.Count - 1; ++i)
        {
            var time = duration * acc[i] / distance;
            hopper.pathPoints.Add(new Hopper.PathPoint() { time = time, transform = pathPoints[i].transform });
        }

        int steps = Mathf.CeilToInt(duration * stepsPerSecond);

        hopper.gaits.Add(HopperAPI.Gait.Stand);
        for (int i = 0; i < steps; ++i)
        {
            HopperAPI.Gait gait = HopperAPI.Gait.Stand;
            var eeCount = hopper.model.eeCount;

            if (eeCount == 1) gait = nominalGait;
            else if (eeCount == 2) gait = nominalGait;
            else if (eeCount == 4) gait = (i == steps - 1) ? endGait : nominalGait;

            hopper.gaits.Add(gait);
        }
        hopper.gaits.Add(HopperAPI.Gait.Stand);

        var last = pathPoints[pathPoints.Count - 1].transform;
        target.position = last.position;
        target.rotation = last.rotation;

        hopper.Optimize();

        pathPoints.Clear();
    }
}
