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

    HopperAPI.Bound bound = new HopperAPI.Bound();
    HopperAPI.State state = new HopperAPI.State();
    Quaternion prevTargetRotation;

    void Awake()
    {
        session = HopperAPI.CreateSession();
    }

    void Start()
    {
        SetBound();
        HopperAPI.StartOptimization(session);
    }

    void Update()
    {
        UpdateStates();

        if (timer >= duration)
        {
            timer = 0;
            SetBound();
            HopperAPI.StartOptimization(session);
        }
    }

    void OnDestroy()
    {
        HopperAPI.EndSession(session);
    }

    void SetBound()
    {
        bound.initialBaseLinearPosition = body.position;
        bound.initialBaseLinearVelocity = state.baseLinearVelocity;
        bound.initialBaseAngularPosition = prevTargetRotation.eulerAngles;
        bound.initialBaseAngularVelocity = state.baseAngularVelocity;

        bound.finalBaseLinearPosition = target.position;
        bound.finalBaseAngularPosition = target.rotation.eulerAngles;
        bound.initialEEPosition = endEffector.position;
        bound.duration = 2.4f;

        prevTargetRotation = target.rotation;

        HopperAPI.SetBound(session, ref bound);
    }

    void UpdateStates()
    {
        if (timer >= duration) return;

        if (HopperAPI.GetSolution(session, timer, out state))
        {
            body.position = state.baseLinearPosition;
            body.rotation = Quaternion.Euler(state.baseAngularPosition);
            endEffector.position = state.eeMotion;

            timer += Time.deltaTime;
        }
    }
}