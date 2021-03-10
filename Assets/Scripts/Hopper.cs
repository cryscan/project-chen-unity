using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hopper : MonoBehaviour
{
    [SerializeField] HopperAPI.RobotModel model;
    [SerializeField] Transform body, target;
    [SerializeField] Transform[] endEffectors;
    [SerializeField] float duration = 2.4f;

    int session;
    int[] sessions = new int[2];
    int current = 0;

    float timer = 0;

    HopperAPI.Bound bound = new HopperAPI.Bound();
    HopperAPI.State state = new HopperAPI.State();
    HopperAPI.ModelInfo modelInfo;

    Quaternion prevTargetRotation;

    void Awake()
    {
        session = HopperAPI.CreateSession(model);
        modelInfo = HopperAPI.GetModelInfo(session);
    }

    void Start()
    {
        InitStance();
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

    void OnDrawGizmos()
    {
        DrawEndEffectorLimits();
    }

    void OnDestroy()
    {
        HopperAPI.EndSession(session);
    }

    void DrawEndEffectorLimits()
    {
        Gizmos.matrix = body.localToWorldMatrix;
        for (int id = 0; id < modelInfo.eeCount; ++id)
            Gizmos.DrawWireCube(modelInfo.nominalStance[id], modelInfo.maxDeviation * 2);
    }

    void InitStance()
    {
        var stanceHeight = Enumerable.Average(modelInfo.nominalStance.Select(x => -x.y));

        {
            var position = body.position;
            position.y = stanceHeight;
            body.position = position;
        }

        for (int id = 0; id < modelInfo.eeCount; ++id)
        {
            var position = modelInfo.nominalStance[id] + body.position;
            endEffectors[id].position = position;
        }

        for (int id = modelInfo.eeCount; id < endEffectors.Length; ++id)
            endEffectors[id].gameObject.SetActive(false);
    }

    void SetBound()
    {
        bound.initialBaseLinearPosition = body.position;
        bound.initialBaseLinearVelocity = state.baseLinearVelocity;
        bound.initialBaseAngularPosition = prevTargetRotation.eulerAngles;
        bound.initialBaseAngularVelocity = state.baseAngularVelocity;

        bound.finalBaseLinearPosition = target.position;
        bound.finalBaseAngularPosition = target.rotation.eulerAngles;

        bound.initialEEPositions = new Vector3[modelInfo.eeCount];
        for (int id = 0; id < modelInfo.eeCount; ++id)
            bound.initialEEPositions[id] = endEffectors[id].position;

        // bound.maxIter = 20;
        bound.maxCpuTime = duration;

        prevTargetRotation = target.rotation;

        HopperAPI.SetBound(session, bound);
    }

    void UpdateStates()
    {
        if (timer >= duration) return;

        if (HopperAPI.GetSolution(session, timer, out state))
        {
            body.position = state.baseLinearPosition;
            body.rotation = Quaternion.Euler(state.baseAngularPosition);
            for (int id = 0; id < HopperAPI.GetEECount(session); ++id)
                endEffectors[id].position = state.eeMotions[id];

            timer += Time.deltaTime;
        }
    }
}