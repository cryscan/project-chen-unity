using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hopper : MonoBehaviour
{
    [SerializeField] RobotModelObject robotModelObject;
    [SerializeField] Transform body;
    [SerializeField] Transform endEffectorParent;
    [SerializeField] Transform target;

    [SerializeField] float duration = 2.4f;

    Transform[] endEffectors;

    int session;

    float timer = 0;

    HopperAPI.Bound bound = new HopperAPI.Bound();
    HopperAPI.State state = new HopperAPI.State();
    HopperAPI.ModelInfo modelInfo;

    void Awake()
    {
        session = HopperAPI.CreateSession(robotModelObject.model);
        modelInfo = HopperAPI.GetModelInfo(session);

        endEffectors = Enumerable.Range(0, endEffectorParent.childCount).Select(x => endEffectorParent.GetChild(x)).ToArray();
    }

    void Start()
    {
        InitStance();
        StartOptimization();
    }

    void Update()
    {
        UpdateStates();

        /*
        if (timer >= duration)
        {
            timer = 0;
            SetBound();
            HopperAPI.StartOptimization(session);
        }
        */
    }

    void OnDrawGizmos()
    {
        DrawEndEffectorLimits();
    }

    void OnDestroy()
    {
        HopperAPI.EndSession(session);
    }

    public void StartOptimization()
    {
        timer = 0;
        SetBound();
        HopperAPI.StartOptimization(session);
    }

    public bool SolutionReady() => HopperAPI.SolutionReady(session);

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
        bound.initialBaseAngularPosition = state.baseAngularPosition;
        bound.initialBaseAngularVelocity = state.baseAngularVelocity;

        bound.finalBaseLinearPosition = target.position;
        bound.finalBaseAngularPosition = target.rotation.eulerAngles;

        bound.initialEEPositions = new Vector3[modelInfo.eeCount];
        for (int id = 0; id < modelInfo.eeCount; ++id)
            bound.initialEEPositions[id] = endEffectors[id].position;

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