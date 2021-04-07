using System.Linq;
using UnityEngine;
using Dynamatica.Runtime;

namespace Dynamatica.Unity
{
    using Options = Runtime.Options;

    public class Model
    {
        public Vector3[] nominalStance;
        public Vector3 maxDeviation;

        public double mass;
        public int eeCount;

        public static implicit operator Model(Runtime.Model model_)
        {
            var model = new Model();

            model.mass = model_.mass;
            model.eeCount = model_.eeCount;

            var query = from id in Enumerable.Range(0, model.eeCount)
                        let array = model_.nominalStance.Skip(id * 3).Take(3).ToArray()
                        select array.ToLinear();
            model.nominalStance = query.ToArray();

            model.maxDeviation = model_.maxDeviation.ToLinear().Abs();

            return model;
        }
    }

    public class Parameters
    {
        public Vector3 initialBaseLinearPosition;
        public Vector3 initialBaseLinearVelocity;
        public Vector3 initialBaseAngularPosition;
        public Vector3 initialBaseAngularVelocity;

        public Vector3 finalBaseLinearPosition;
        public Vector3 finalBaseLinearVelocity;
        public Vector3 finalBaseAngularPosition;
        public Vector3 finalBaseAngularVelocity;

        public Vector3[] initialEEPositions;

        public Dim3D boundsFinalLinearPosition = Dim3D.X | Dim3D.Z;
        public Dim3D boundsFinalLinearVelocity = Dim3D.X | Dim3D.Y | Dim3D.Z;
        public Dim3D boundsFinalAngularPosition = Dim3D.X | Dim3D.Y | Dim3D.Z;
        public Dim3D boundsFinalAngularVelocity = Dim3D.X | Dim3D.Y | Dim3D.Z;

        public static implicit operator Runtime.Parameters(Parameters parameters)
        {
            var parameters_ = new Runtime.Parameters();

            parameters_.initialBaseLinearPosition = parameters.initialBaseLinearPosition.ToLinear();
            parameters_.initialBaseLinearVelocity = parameters.initialBaseLinearVelocity.ToLinear();
            parameters_.initialBaseAngularPosition = parameters.initialBaseAngularPosition.ToAngular();
            parameters_.initialBaseAngularVelocity = parameters.initialBaseAngularVelocity.ToAngular();

            parameters_.finalBaseLinearPosition = parameters.finalBaseLinearPosition.ToLinear();
            parameters_.finalBaseLinearVelocity = parameters.finalBaseLinearVelocity.ToLinear();
            parameters_.finalBaseAngularPosition = parameters.finalBaseAngularPosition.ToAngular();
            parameters_.finalBaseAngularVelocity = parameters.finalBaseAngularVelocity.ToAngular();

            var query = from vector in parameters.initialEEPositions
                        from x in vector.ToLinear()
                        select x;
            parameters_.initialEEPositions = query.ToArray();

            parameters_.boundsFinalLinearPosition = (byte)parameters.boundsFinalLinearPosition;
            parameters_.boundsFinalLinearVelocity = (byte)parameters.boundsFinalLinearVelocity;
            parameters_.boundsFinalAngularPosition = (byte)parameters.boundsFinalAngularPosition;
            parameters_.boundsFinalAngularVelocity = (byte)parameters.boundsFinalAngularVelocity;

            return parameters_;
        }
    }

    public class PathPoint
    {
        public float time;
        public Vector3 linear, angular;
        public Dim6D bounds;

        public static implicit operator Runtime.PathPoint(PathPoint pathPoint)
        {
            var pathPoint_ = new Runtime.PathPoint();
            pathPoint_.time = pathPoint.time;
            pathPoint_.linear = pathPoint.linear.ToLinear();
            pathPoint_.angular = pathPoint.angular.ToAngular();
            pathPoint_.bounds = (byte)pathPoint.bounds;
            return pathPoint_;
        }
    }

    public class State
    {
        public Vector3 baseLinearPosition;
        public Vector3 baseLinearVelocity;
        public Vector3 baseAngularPosition;
        public Vector3 baseAngularVelocity;

        public Vector3[] eeMotions;
        public Vector3[] eeForces;
        public bool[] contacts;

        public static implicit operator State(Runtime.State state_)
        {
            var state = new State();

            state.baseLinearPosition = state_.baseLinearPosition.ToLinear();
            state.baseLinearVelocity = state_.baseLinearVelocity.ToLinear();
            state.baseAngularPosition = state_.baseAngularPosition.ToAngular();
            state.baseAngularVelocity = state_.baseAngularVelocity.ToAngular();

            state.eeMotions = new Vector3[4];
            state.eeForces = new Vector3[4];
            state.contacts = new bool[4];

            for (int id = 0; id < 4; ++id)
            {
                state.eeMotions[id] = state_.eeMotions.Skip(id * 3).Take(3).ToArray().ToLinear();
                state.eeForces[id] = state_.eeForces.Skip(id * 3).Take(3).ToArray().ToLinear();
                state.contacts[id] = state_.contacts[id];
            }

            return state;
        }
    }

    static class Utils
    {
        public static Vector3 ToLinear(this double[] array) => new Vector3(-(float)array[1], (float)array[2], (float)array[0]);

        public static double[] ToLinear(this Vector3 vector)
        {
            var array = new double[3];
            array[0] = vector.z;
            array[1] = -vector.x;
            array[2] = vector.y;
            return array;
        }

        public static Vector3 ToAngular(this double[] array)
        {
            var vector = new Vector3((float)array[1], -(float)array[2], -(float)array[0]);
            return vector * Mathf.Rad2Deg;
        }

        public static double[] ToAngular(this Vector3 vector)
        {
            vector *= Mathf.Deg2Rad;
            var array = new double[3];
            array[0] = -vector.z;
            array[1] = vector.x;
            array[2] = -vector.y;
            return array;
        }

        public static Vector3 Abs(this Vector3 vector)
        {
            vector.x = Mathf.Abs(vector.x);
            vector.y = Mathf.Abs(vector.y);
            vector.z = Mathf.Abs(vector.z);
            return vector;
        }
    }

    public class Session
    {
        public int id { get; private set; }

        public bool dirty { get; private set; } = false;
        public bool ready { get => Core.SolutionReady(id); }

        public Session(Robot robot)
        {
            id = Core.CreateSession(robot);
            Debug.Log($"[Dynamatica] Session {id} Created");
        }

        ~Session()
        {
            Core.EndSession(id);
            Debug.Log($"[Dynamatica] Session {id} Ended");
        }

        public Model model
        {
            get
            {
                Runtime.Model model;
                Core.GetModel(id, out model);
                return model;
            }
        }

        public void SetTerrain(Terrain terrain) => Core.SetTerrain(id, terrain.id);
        public void SetParams(Parameters parameters) => Core.SetParams(id, parameters);
        public void SetOptions(Options options) => Core.SetOptions(id, options);
        public void SetDuration(double duration) => Core.SetDuration(id, duration);

        public void PushPathPoint(PathPoint pathPoint) => Core.PushPathPoint(id, pathPoint);
        public void PushGait(Gait gait) => Core.PushGait(id, gait);

        public void StartOptimization()
        {
            dirty = true;
            Core.StartOptimization(id);
        }

        public State GetState(double time)
        {
            Runtime.State state;
            Core.GetSolutionState(id, time, out state);
            return state;
        }
    }

    public class Terrain
    {
        public int id { get; private set; }

        public Vector3 origin { get; private set; }
        public double unitSize { get; private set; }
        public uint x { get; private set; }
        public uint y { get; private set; }

        public Terrain(Vector3 origin, uint x, uint y, double unitSize)
        {
            this.origin = origin;
            this.unitSize = unitSize;

            this.x = x;
            this.y = y;

            var pos = origin.ToLinear();
            id = Core.CreateTerrain(pos[0], pos[1], pos[2], x, y, unitSize);
            Debug.Log($"[Dynamatica] Terrain {id} Created");
        }

        ~Terrain()
        {
            Core.EndTerrain(id);
            Debug.Log($"[Dynamatica] Terrain {id} Ended");
        }

        public void SetHeight(uint x, uint y, double height)
        {
            if (x > this.x || y > this.y) return;
            Core.SetHeight(id, x, y, height);
        }

        public double GetHeight(Vector2 pos) => Core.GetHeight(id, pos.x, pos.y);

        public Vector2 GetHeightDerivatives(Vector2 pos)
        {
            double dx, dy;
            Core.GetHeightDerivatives(id, pos.x, pos.y, out dx, out dy);
            return new Vector2((float)dx, (float)dy);
        }
    }
}
