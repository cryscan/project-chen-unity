using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hopper : MonoBehaviour
{
    [SerializeField] HopperAPI.Robot robot;
    [SerializeField] Transform body;
    [SerializeField] Transform endEffectorParent;
    [SerializeField] Transform target;

    [SerializeField] float duration = 2.4f;
    [SerializeField] float cut = 2.4f;

    Transform[] endEffectors;

    int session;

    float timer = 0;

    HopperAPI.State state = new HopperAPI.State();
    HopperAPI.Model model;

    void Start()
    {
        session = HopperAPI.CreateSession(robot);
        model = HopperAPI.GetRobotModel(session);

        endEffectors = Enumerable.Range(0, endEffectorParent.childCount).Select(x => endEffectorParent.GetChild(x)).ToArray();

        Debug.Log($"Session {session} Created");

        InitStance();
    }

    void Update()
    {
        UpdateStates();
    }

    void OnDrawGizmos()
    {
        DrawEndEffectorLimits();
    }

    void OnDestroy()
    {
        HopperAPI.EndSession(session);
    }

    public void SetRobot(HopperAPI.Robot robot) => this.robot = robot;

    public void StartOptimization()
    {
        SetBound();
        SetOption();

        timer = 0;
        HopperAPI.StartOptimization(session);
    }

    public bool SolutionReady() => HopperAPI.SolutionReady(session);

    void DrawEndEffectorLimits()
    {
        if (model == null) return;

        Gizmos.matrix = body.localToWorldMatrix;
        for (int id = 0; id < model.eeCount; ++id)
            Gizmos.DrawWireCube(model.nominalStance[id], model.maxDeviation * 2);
    }

    void InitStance()
    {
        var stanceHeight = Enumerable.Average(model.nominalStance.Select(x => -x.y));

        {
            var position = body.position;
            position.y = stanceHeight;
            body.position = position;
        }

        for (int id = 0; id < model.eeCount; ++id)
        {
            var position = model.nominalStance[id] + body.position;
            endEffectors[id].position = position;
        }

        for (int id = model.eeCount; id < endEffectors.Length; ++id)
            endEffectors[id].gameObject.SetActive(false);
    }

    void SetBound()
    {
        var parameters = new HopperAPI.Parameters();

        parameters.initialBaseLinearPosition = body.position;
        parameters.initialBaseLinearVelocity = state.baseLinearVelocity;
        parameters.initialBaseAngularPosition = state.baseAngularPosition;
        parameters.initialBaseAngularVelocity = state.baseAngularVelocity;

        parameters.finalBaseLinearPosition = target.position;
        parameters.finalBaseAngularPosition = target.rotation.eulerAngles;

        var distance = Vector3.Distance(target.position, body.position);
        var velocity = target.forward * distance / duration;
        parameters.finalBaseLinearVelocity = velocity;

        parameters.initialEEPositions = new Vector3[model.eeCount];
        for (int id = 0; id < model.eeCount; ++id)
            parameters.initialEEPositions[id] = endEffectors[id].position;

        HopperAPI.SetParams(session, parameters);
    }

    void SetOption()
    {
        var option = new HopperAPI.Options();
        // option.maxCpuTime = cut;
        HopperAPI.SetOptions(session, option);
    }

    void UpdateStates()
    {
        if (timer >= cut) return;

        if (HopperAPI.GetSolutionState(session, timer, out state))
        {
            body.position = state.baseLinearPosition;
            body.rotation = Quaternion.Euler(state.baseAngularPosition);
            for (int id = 0; id < HopperAPI.GetEECount(session); ++id)
                endEffectors[id].position = state.eeMotions[id];

            timer += Time.deltaTime;
        }
    }
}