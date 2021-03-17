using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hopper : MonoBehaviour
{
    [System.Serializable]
    struct PathPoint
    {
        public float time;
        public Transform transform;
    }

    [SerializeField] HopperAPI.Robot robot;
    [SerializeField] Transform body;
    [SerializeField] Transform eeParent;
    [SerializeField] Transform target;
    [SerializeField] PathPoint[] pathPoints;

    [SerializeField] float duration = 2.4f;
    [SerializeField] float cut = 2.4f;

    Transform[] ees;

    float timer = 0;

    HopperAPI.Session session;
    HopperAPI.State state = new HopperAPI.State();
    HopperAPI.Model model;

    void Start()
    {
        session = new HopperAPI.Session(robot);
        model = session.GetModel();

        ees = Enumerable.Range(0, eeParent.childCount).Select(x => eeParent.GetChild(x)).ToArray();

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

    public void SetRobot(HopperAPI.Robot robot) => this.robot = robot;

    public void Optimize()
    {
        session.SetParams(GetParams());
        session.SetOptions(GetOptions());

        timer = 0;
        session.Optimize();
    }

    public bool SolutionReady() => session.ready;

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
            ees[id].position = position;
        }

        for (int id = model.eeCount; id < ees.Length; ++id)
            ees[id].gameObject.SetActive(false);
    }

    HopperAPI.Parameters GetParams()
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
            parameters.initialEEPositions[id] = ees[id].position;

        return parameters;
    }

    HopperAPI.Options GetOptions()
    {
        var option = new HopperAPI.Options();
        // option.maxCpuTime = cut;
        return option;
    }

    void UpdateStates()
    {
        if (timer >= cut) return;

        if (session.ready)
        {
            state = session.GetState(timer);
            body.position = state.baseLinearPosition;
            body.rotation = Quaternion.Euler(state.baseAngularPosition);
            for (int id = 0; id < model.eeCount; ++id)
                ees[id].position = state.eeMotions[id];

            timer += Time.deltaTime;
        }
    }
}