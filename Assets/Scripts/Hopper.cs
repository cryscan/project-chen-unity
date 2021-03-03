using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hopper : MonoBehaviour
{
    [SerializeField] Transform body, endEffector, target;
    [SerializeField] float duration = 2.4f;

    int session;
    int[] sessions = new int[2];
    int current = 0;

    float timer = 0;

    HopperBackend.Boundary boundary = new HopperBackend.Boundary();
    HopperBackend.Solution solution = new HopperBackend.Solution();
    Quaternion prevTargetRotation;

    void Awake()
    {
        session = HopperBackend.CreateSession();
    }

    void Start()
    {
        SetBoundary();
        HopperBackend.StartOptimization(session);
    }

    void Update()
    {
        UpdateStates();

        if (timer >= duration)
        {
            timer = 0;
            SetBoundary();
            HopperBackend.StartOptimization(session);
        }
    }

    void OnDestroy()
    {
        HopperBackend.EndSession(session);
    }

    void SetBoundary()
    {
        boundary.initialBaseLinearPosition = body.position;
        boundary.initialBaseLinearVelocity = solution.baseLinearVelocity;
        boundary.initialBaseAngularPosition = prevTargetRotation.eulerAngles;
        // boundary.initialBaseAngularVelocity = solution.baseAngularVelocity;

        boundary.finalBaseLinearPosition = target.position;
        boundary.finalBaseAngularPosition = target.rotation.eulerAngles;
        boundary.initialEEPosition = endEffector.position;
        boundary.duration = 2.4;

        prevTargetRotation = target.rotation;

        HopperBackend.SetBoundary(session, ref boundary);
    }

    void UpdateStates()
    {
        if (timer >= duration) return;

        if (HopperBackend.GetSolution(session, timer, out solution))
        {
            body.position = solution.baseLinearPosition;
            body.rotation = Quaternion.Euler(solution.baseAngularPosition);
            endEffector.position = solution.eeMotion;

            timer += Time.deltaTime;
        }
    }
}