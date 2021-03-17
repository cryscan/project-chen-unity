﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    [Header("Configure")]
    [SerializeField] LayerMask layer;
    [SerializeField] Hopper hopper;
    [SerializeField] Transform body;
    [SerializeField] Transform target;
    [SerializeField] GameObject pathPointPrefab;

    [Header("Trajectory")]
    [SerializeField] float speed = 1;
    [SerializeField] float stepsPerSecond = 1;

    Camera _camera;
    bool locked = false;

    List<GameObject> pathPoints = new List<GameObject>();

    void Awake()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        UpdateTransform();

        if (Input.GetKeyDown(KeyCode.G))
            hopper.optimizeGaits = !hopper.optimizeGaits;

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
    }

    void AddPathPoint()
    {
        var pathPoint = Instantiate(pathPointPrefab, transform.position, transform.rotation);
        pathPoints.Add(pathPoint);
    }

    HopperAPI.Gait ChooseGait()
    {
        if (hopper.model.eeCount == 1) return HopperAPI.Gait.Hop1;
        else if (hopper.model.eeCount == 2) return HopperAPI.Gait.Walk1;
        else if (hopper.model.eeCount == 4) return HopperAPI.Gait.Walk2;
        return HopperAPI.Gait.Walk1;
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

        float duration = distance / speed;
        hopper.duration = duration;

        for (int i = 0; i < pathPoints.Count - 1; ++i)
        {
            var time = duration * acc[i] / distance;
            hopper.pathPoints.Add(new Hopper.PathPoint() { time = time, transform = pathPoints[i].transform });
        }

        int steps = Mathf.CeilToInt(duration * stepsPerSecond);
        hopper.gaits.Add(HopperAPI.Gait.Stand);
        for (int i = 0; i < steps; ++i) hopper.gaits.Add(ChooseGait());
        hopper.gaits.Add(HopperAPI.Gait.Stand);

        var last = pathPoints[pathPoints.Count - 1].transform;
        target.position = last.position;
        target.rotation = last.rotation;

        hopper.Optimize();

        pathPoints.Clear();
    }
}
