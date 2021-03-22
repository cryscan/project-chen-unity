using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionSynth
{
    [System.Serializable]
    public enum Robot { Monoped, Biped, Hyq, Anymal }

    [System.Serializable]
    public enum Gait
    {
        Stand = 0, Flight,
        Walk1, Walk2, Walk2E,
        Run2, Run2E, Run1, Run1E, Run3, Run3E,
        Hop1, Hop1E, Hop2, Hop3, Hop3E, Hop5, Hop5E
    }

    [System.Serializable]
    [System.Flags]
    public enum Dim3D : byte { Z = 1, X = 2, Y = 4 };

    [System.Serializable]
    [System.Flags]
    public enum Dim6D : byte { AZ = 1, AX = 2, AY = 4, LZ = 8, LX = 16, LY = 32 };

    public static class Core
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Model
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] nominalStance;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] maxDeviation;

            public double mass;
            public int eeCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Parameters
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] initialBaseLinearPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] initialBaseLinearVelocity;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] initialBaseAngularPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] initialBaseAngularVelocity;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] finalBaseLinearPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] finalBaseLinearVelocity;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] finalBaseAngularPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] finalBaseAngularVelocity;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] initialEEPositions;

            public byte boundsFinalLinearPosition;
            public byte boundsFinalLinearVelocity;
            public byte boundsFinalAngularPosition;
            public byte boundsFinalAngularVelocity;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class PathPoint
        {
            public double time;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] linear;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] angular;
            public byte bounds;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class State
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseLinearPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseLinearVelocity;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseAngularPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseAngularVelocity;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] eeMotions;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] eeForces;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public bool[] contacts;
        }


        /* Interface to the C library */

        [DllImport("hopper", EntryPoint = "create_session")]
        public static extern int CreateSession(Robot model = 0);

        [DllImport("hopper", EntryPoint = "end_session")]
        public static extern void EndSession(int session);

        [DllImport("hopper", EntryPoint = "get_model")]
        public static extern void GetModel(int session, out Model model);

        [DllImport("hopper", EntryPoint = "get_ee_count")]
        public static extern int GetEECount(int session);

        [DllImport("hopper", EntryPoint = "set_terrain")]
        public static extern void SetTerrain(int session, int terrain);

        [DllImport("hopper", EntryPoint = "set_params")]
        public static extern void SetParams(int session, Parameters parameters);

        [DllImport("hopper", EntryPoint = "set_options")]
        public static extern void SetOptions(int session, Options options);

        [DllImport("hopper", EntryPoint = "set_duration")]
        public static extern void SetDuration(int session, double duration);

        [DllImport("hopper", EntryPoint = "push_path_point")]
        public static extern void PushPathPoint(int session, PathPoint pathPoint);

        [DllImport("hopper", EntryPoint = "push_gait")]
        public static extern void PushGait(int session, Gait gait);

        [DllImport("hopper", EntryPoint = "start_optimization")]
        public static extern void StartOptimization(int session);

        [DllImport("hopper", EntryPoint = "solution_ready")]
        public static extern bool SolutionReady(int session);

        [DllImport("hopper", EntryPoint = "get_solution_state")]
        public static extern bool GetSolutionState(int session, double time, out State state);

        [DllImport("hopper", EntryPoint = "create_terrain")]
        public static extern int CreateTerrain(double posX, double posY, double posZ, uint x, uint y, double unitSize);

        [DllImport("hopper", EntryPoint = "end_terrain")]
        public static extern void EndTerrain(int terrain);

        [DllImport("hopper", EntryPoint = "set_height")]
        public static extern void SetHeight(int terrain, uint x, uint y, double height);

        [DllImport("hopper", EntryPoint = "get_height")]
        public static extern double GetHeight(int terrain, double x, double y);

        [DllImport("hopper", EntryPoint = "get_height_derivatives")]
        public static extern void GetHeightDerivatives(int terrain, double x, double y, out double dx, out double dy);
    }

    public class Model
    {
        public Vector3[] nominalStance;
        public Vector3 maxDeviation;

        public double mass;
        public int eeCount;

        public static implicit operator Model(Core.Model model_)
        {
            var model = new Model();

            model.mass = model_.mass;
            model.eeCount = model_.eeCount;

            var query = from id in Enumerable.Range(0, 4)
                        let array = model_.nominalStance.Skip(id * 3).Take(3).ToArray()
                        select array.LinearConversion();
            model.nominalStance = query.ToArray();

            model.maxDeviation = model_.maxDeviation.LinearConversion().Abs();

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

        public static implicit operator Core.Parameters(Parameters parameters)
        {
            var parameters_ = new Core.Parameters();

            parameters_.initialBaseLinearPosition = parameters.initialBaseLinearPosition.LinearConversion();
            parameters_.initialBaseLinearVelocity = parameters.initialBaseLinearVelocity.LinearConversion();
            parameters_.initialBaseAngularPosition = parameters.initialBaseAngularPosition.AngularConversion();
            parameters_.initialBaseAngularVelocity = parameters.initialBaseAngularVelocity.AngularConversion();

            parameters_.finalBaseLinearPosition = parameters.finalBaseLinearPosition.LinearConversion();
            parameters_.finalBaseLinearVelocity = parameters.finalBaseLinearVelocity.LinearConversion();
            parameters_.finalBaseAngularPosition = parameters.finalBaseAngularPosition.AngularConversion();
            parameters_.finalBaseAngularVelocity = parameters.finalBaseAngularVelocity.AngularConversion();

            var query = from vector in parameters.initialEEPositions
                        from x in vector.LinearConversion()
                        select x;
            parameters_.initialEEPositions = query.ToArray();

            parameters_.boundsFinalLinearPosition = (byte)parameters.boundsFinalLinearPosition;
            parameters_.boundsFinalLinearVelocity = (byte)parameters.boundsFinalLinearVelocity;
            parameters_.boundsFinalAngularPosition = (byte)parameters.boundsFinalAngularPosition;
            parameters_.boundsFinalAngularVelocity = (byte)parameters.boundsFinalAngularVelocity;

            return parameters_;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class Options
    {
        public double maxCpuTime;
        public int maxIter;
        public bool optimizePhaseDurations;
    }

    public class PathPoint
    {
        public float time;
        public Vector3 linear, angular;
        public Dim6D bounds;

        public static implicit operator Core.PathPoint(PathPoint pathPoint)
        {
            var pathPoint_ = new Core.PathPoint();
            pathPoint_.time = pathPoint.time;
            pathPoint_.linear = pathPoint.linear.LinearConversion();
            pathPoint_.angular = pathPoint.angular.AngularConversion();
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

        public static implicit operator State(Core.State state_)
        {
            var state = new State();

            state.baseLinearPosition = state_.baseLinearPosition.LinearConversion();
            state.baseLinearVelocity = state_.baseLinearVelocity.LinearConversion();
            state.baseAngularPosition = state_.baseAngularPosition.AngularConversion();
            state.baseAngularVelocity = state_.baseAngularVelocity.AngularConversion();

            state.eeMotions = new Vector3[4];
            state.eeForces = new Vector3[4];
            state.contacts = new bool[4];

            for (int id = 0; id < 4; ++id)
            {
                state.eeMotions[id] = state_.eeMotions.Skip(id * 3).Take(3).ToArray().LinearConversion();
                state.eeForces[id] = state_.eeForces.Skip(id * 3).Take(3).ToArray().LinearConversion();
                state.contacts[id] = state_.contacts[id];
            }

            return state;
        }
    }

    static class Utils
    {
        public static Vector3 LinearConversion(this double[] array) => new Vector3(-(float)array[1], (float)array[2], (float)array[0]);

        public static double[] LinearConversion(this Vector3 vector)
        {
            var array = new double[3];
            array[0] = vector.z;
            array[1] = -vector.x;
            array[2] = vector.y;
            return array;
        }

        public static Vector3 AngularConversion(this double[] array)
        {
            var vector = new Vector3((float)array[1], -(float)array[2], -(float)array[0]);
            return vector * Mathf.Rad2Deg;
        }

        public static double[] AngularConversion(this Vector3 vector)
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
            Debug.Log($"[Motion Synth] Session {id} Created");
        }

        ~Session()
        {
            Core.EndSession(id);
            Debug.Log($"[Motion Synth] Session {id} Ended");
        }

        public Model model
        {
            get
            {
                var model = new Core.Model();
                Core.GetModel(id, out model);
                return model;
            }
        }

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
            var state = new Core.State();
            Core.GetSolutionState(id, time, out state);
            return state;
        }
    }

    public class Terrain
    {
        public int id { get; private set; }

        Vector3 origin;
        double unitSize;
        uint x, y;

        public Terrain(Vector3 origin, uint x, uint y, double unitSize)
        {
            this.origin = origin;
            this.unitSize = unitSize;

            this.x = x;
            this.y = y;

            var pos = origin.LinearConversion();
            id = Core.CreateTerrain(pos[0], pos[1], pos[2], x, y, unitSize);
            Debug.Log($"[Motion Synth] Terrain {id} Created");
        }

        ~Terrain()
        {
            Core.EndTerrain(id);
            Debug.Log($"[Motion Synth] Terrain {id} Ended");
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