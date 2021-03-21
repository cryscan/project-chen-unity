using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hopper : MonoBehaviour
{
    [System.Serializable]
    public struct PathPoint
    {
        public float time;
        public Transform transform;
    }

    [SerializeField] TerrainBuilder terrainBuilder;

    [Header("Robot")]
    [SerializeField] HopperAPI.Robot robot;
    [SerializeField] Transform body;
    [SerializeField] Transform eeParent;
    [SerializeField] TerrainFollower terrainFollower;

    [SerializeField] Material stanceMaterial, flightMaterial;

    [Header("Trajectory")]
    public bool optimizeGaits = false;
    public List<PathPoint> pathPoints;
    public Transform target;

    public float duration = 2.4f;
    public List<HopperAPI.Gait> gaits;

    HierarchyRecorder recorder;
    Vector3 rootPosition;

    Transform[] ee;
    MeshRenderer[] eeMeshRenderers;

    public float timer { get; set; } = 0;

    public HopperAPI.Session session { get; private set; }
    public HopperAPI.Model model { get; private set; }
    HopperAPI.State state = new HopperAPI.State();

    void Awake()
    {
        recorder = GetComponent<HierarchyRecorder>();

        ee = Enumerable.Range(0, eeParent.childCount).Select(x => eeParent.GetChild(x)).ToArray();
        eeMeshRenderers = ee.Select(x => x.GetComponentInChildren<MeshRenderer>()).ToArray();

        if (terrainFollower) terrainFollower.terrainBuilder = terrainBuilder;
    }

    void Start()
    {
        Reset();
        InitStance();
    }

    void Update()
    {
        UpdateStates();
    }

    void OnDrawGizmos()
    {
        DrawEELimits();
    }

    public void SetRobot(HopperAPI.Robot robot) => this.robot = robot;

    public void Optimize()
    {
        session.SetParams(GetParams());
        session.SetOptions(GetOptions());
        session.SetDuration(duration);

        foreach (var p in pathPoints)
        {
            var pathPoint = new HopperAPI.PathPoint();
            pathPoint.time = p.time;
            pathPoint.linear = p.transform.position;
            pathPoint.angular = p.transform.rotation.eulerAngles;
            session.PushPathPoint(pathPoint);
        }

        foreach (var gait in gaits)
            session.PushGait(gait);

        timer = 0;
        session.StartOptimization();
    }

    public void Reset()
    {
        session = new HopperAPI.Session(robot);
        model = session.GetModel();

        if (terrainBuilder)
            session.SetTerrain(terrainBuilder.terrain);

        pathPoints.Clear();
        gaits.Clear();
    }

    void DrawEELimits()
    {
        if (model == null) return;

        Gizmos.matrix = body.localToWorldMatrix;
        for (int id = 0; id < model.eeCount; ++id)
            Gizmos.DrawWireCube(model.nominalStance[id], model.maxDeviation * 2);
    }

    void InitStance()
    {
        var stanceHeight = Enumerable.Average(model.nominalStance.Select(x => -x.y));

        var translation = new Vector3(0, stanceHeight, 0);
        body.Translate(translation - body.localPosition, Space.Self);

        for (int id = 0; id < model.eeCount; ++id)
            ee[id].Translate(model.nominalStance[id] + translation - ee[id].localPosition, Space.Self);

        for (int id = model.eeCount; id < ee.Length; ++id)
            ee[id].gameObject.SetActive(false);

        state.baseLinearPosition = body.transform.position;
        state.baseAngularPosition = body.transform.rotation.eulerAngles;
    }

    HopperAPI.Parameters GetParams()
    {
        var parameters = new HopperAPI.Parameters();

        parameters.initialBaseLinearPosition = state.baseLinearPosition;
        parameters.initialBaseLinearVelocity = state.baseLinearVelocity;
        parameters.initialBaseAngularPosition = state.baseAngularPosition;
        parameters.initialBaseAngularVelocity = state.baseAngularVelocity;

        parameters.finalBaseLinearPosition = target.position;
        parameters.finalBaseAngularPosition = target.rotation.eulerAngles;

        // var distance = Vector3.Distance(target.position, body.position);
        // var velocity = target.forward * distance / duration;
        // parameters.finalBaseLinearVelocity = velocity;

        parameters.initialEEPositions = new Vector3[model.eeCount];
        for (int id = 0; id < model.eeCount; ++id)
            parameters.initialEEPositions[id] = ee[id].position;

        return parameters;
    }

    HopperAPI.Options GetOptions()
    {
        var option = new HopperAPI.Options();
        option.maxCpuTime = 180;
        option.optimizePhaseDurations = optimizeGaits;
        return option;
    }

    void UpdateStates()
    {
        if (timer >= duration)
        {
            recorder.EndRecording();
            return;
        }

        if (session.ready)
        {
            state = session.GetState(timer);

            // Update root position.
            rootPosition = Vector3.zero;
            for (int id = 0; id < model.eeCount; ++id) rootPosition += state.eeMotions[id];
            rootPosition /= model.eeCount;

            transform.position = rootPosition;
            transform.rotation = Quaternion.Euler(0, state.baseAngularPosition.y, 0);

            body.position = state.baseLinearPosition;
            body.rotation = Quaternion.Euler(state.baseAngularPosition);

            for (int id = 0; id < model.eeCount; ++id)
            {
                ee[id].position = state.eeMotions[id];

                var force = state.eeForces[id].magnitude;
                eeMeshRenderers[id].material = force > 0 ? stanceMaterial : flightMaterial;
            }

            timer += Time.deltaTime;
            recorder.BeginRecording();
        }
    }
}