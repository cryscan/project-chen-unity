using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Dynamatica.Runtime;

namespace Dynamatica.Unity.Components
{
    [DisallowMultipleComponent]
    public class EndEffector : MonoBehaviour
    {
        public Vector3 force;
        public bool contact { get => force.magnitude > 0; }

        [SerializeField] Material stanceMaterial, flightMaterial;

        MeshRenderer mesh;

        void Awake()
        {
            mesh = GetComponentInChildren<MeshRenderer>();
        }

        void Update()
        {
            mesh.material = contact ? stanceMaterial : flightMaterial;
        }
    }

    [RequireComponent(typeof(HierarchyRecorder))]
    public class Dynamatica : MonoBehaviour
    {
        [Header("Model")]
        public Robot robot;
        [SerializeField] Transform body;

        [Header("Trajectory")]
        public Path path;
        public bool optimizeGaits;

        public Session session { get; private set; }
        public Model model { get; private set; }
        public State state { get; private set; } = new State();

        EndEffector[] ee;
        float timer = 0;

        HierarchyRecorder recorder;

        void Awake()
        {
            ee = GetComponentsInChildren<EndEffector>();
            recorder = GetComponent<HierarchyRecorder>();
        }

        void Start()
        {
            if (!(path?.valid ?? false))
            {
                Debug.LogError($"[Dynamatica] Path must not be null or invalid");
                EditorApplication.isPlaying = false;
            }

            ResetStance();
        }

        void FixedUpdate()
        {
            if (timer > path.duration)
            {
                recorder.EndRecording();
                return;
            }

            if (session.ready)
            {
                state = session.GetState(timer);

                var position = Vector3.zero;
                foreach (var vector in state.eeMotions) position += vector;
                position /= model.eeCount;

                transform.position = position;
                transform.rotation = Quaternion.Euler(0, state.baseAngularPosition.y, 0);

                body.position = state.baseLinearPosition;
                body.rotation = Quaternion.Euler(state.baseAngularPosition);

                for (int id = 0; id < model.eeCount; ++id)
                {
                    ee[id].transform.position = state.eeMotions[id];
                    ee[id].force = state.eeForces[id];
                }

                recorder.Record();
                timer += Time.deltaTime;
            }
        }

        public void ResetStance()
        {
            Reset();
            InitStance();
        }

        public void Optimize()
        {
            Reset();

            session.SetParams(MakeParams());
            session.SetOptions(MakeOptions());
            session.SetDuration(path.duration);

            foreach (var pathPoint in path.pathPoints)
                session.PushPathPoint(pathPoint);

            foreach (var gait in path.gaits)
                session.PushGait(gait);

            session.StartOptimization();
        }

        void Reset()
        {
            session = new Session(robot);
            model = session.model;

            for (int id = 0; id < ee.Length; ++id)
                ee[id].gameObject.SetActive(id < model.eeCount);

            timer = 0;
        }

        void InitStance()
        {
            var start = path.pathPoints.First();
            var position = start.linear;
            var rotation = Quaternion.Euler(start.angular);

            transform.SetPositionAndRotation(position, rotation);

            state.baseLinearPosition = start.linear;
            state.baseLinearVelocity = Vector3.zero;
            state.baseAngularPosition = start.angular;
            state.baseAngularVelocity = Vector3.zero;

            var stanceHeight = Enumerable.Average(model.nominalStance.Select(v => -v.y));
            body.localPosition = new Vector3(0, stanceHeight, 0);

            for (int id = 0; id < model.eeCount; ++id)
            {
                position = model.nominalStance[id];
                position.y += stanceHeight;
                ee[id].transform.localPosition = position;
            }
        }

        Parameters MakeParams()
        {
            var parameters = new Parameters();

            parameters.initialBaseLinearPosition = state.baseLinearPosition;
            parameters.initialBaseLinearVelocity = state.baseLinearVelocity;
            parameters.initialBaseAngularPosition = state.baseAngularPosition;
            parameters.initialBaseAngularVelocity = state.baseAngularVelocity;

            parameters.initialEEPositions = new Vector3[4];
            for (int id = 0; id < model.eeCount; ++id)
            {
                var position = ee[id].transform.position;
                parameters.initialEEPositions[id] = position;
            }

            var target = path.pathPoints.Last();
            parameters.finalBaseLinearPosition = target.linear;
            parameters.finalBaseAngularPosition = target.angular;

            return parameters;
        }

        Options MakeOptions()
        {
            var options = new Options();

            options.maxCpuTime = 180;
            options.optimizePhaseDurations = optimizeGaits;
            return options;
        }
    }
}
