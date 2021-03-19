using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CHopper
{
    [StructLayout(LayoutKind.Sequential)]
    struct Model
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] nominalStance;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] maxDeviation;

        public double mass;
        public int eeCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    class Parameters
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
    class Options
    {
        public double maxCpuTime;
        public int maxIter;
        public bool optimizePhaseDurations;
    }

    [StructLayout(LayoutKind.Sequential)]
    class PathPoint
    {
        public double time;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] linear;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] angular;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct State
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseLinearPosition;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseLinearVelocity;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseAngularPosition;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseAngularVelocity;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] eeMotions;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] eeForces;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public bool[] contacts;
    }
}

public static class HopperAPI
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
    public enum Dim3D { Z, X, Y };

    [DllImport("hopper", EntryPoint = "create_session")]
    public static extern int CreateSession(Robot model = 0);

    [DllImport("hopper", EntryPoint = "end_session")]
    public static extern void EndSession(int session);

    [DllImport("hopper", EntryPoint = "get_model")]
    static extern void GetModel(int session, out CHopper.Model model);

    [DllImport("hopper", EntryPoint = "get_ee_count")]
    public static extern int GetEECount(int session);

    [DllImport("hopper", EntryPoint = "set_terrain")]
    public static extern void SetTerrain(int session, int terrain);

    [DllImport("hopper", EntryPoint = "set_params")]
    static extern void SetParams(int session, CHopper.Parameters parameters);

    [DllImport("hopper", EntryPoint = "set_options")]
    static extern void SetOptions(int session, CHopper.Options options);

    [DllImport("hopper", EntryPoint = "set_duration")]
    public static extern void SetDuration(int session, double duration);

    [DllImport("hopper", EntryPoint = "push_path_point")]
    static extern void PushPathPoint(int session, CHopper.PathPoint pathPoint);

    [DllImport("hopper", EntryPoint = "push_gait")]
    public static extern void PushGait(int session, Gait gait);

    [DllImport("hopper", EntryPoint = "start_optimization")]
    public static extern void StartOptimization(int session);

    [DllImport("hopper", EntryPoint = "solution_ready")]
    public static extern bool SolutionReady(int session);

    [DllImport("hopper", EntryPoint = "get_solution_state")]
    static extern bool GetSolutionState(int session, double time, out CHopper.State state);

    [DllImport("hopper", EntryPoint = "create_terrain")]
    static extern int CreateTerrain(double posX, double posY, double posZ, uint x, uint y, double unitSize);

    [DllImport("hopper", EntryPoint = "end_terrain")]
    public static extern void EndTerrain(int terrain);

    [DllImport("hopper", EntryPoint = "set_height")]
    public static extern void SetHeight(int terrain, uint x, uint y, double height);

    [DllImport("hopper", EntryPoint = "get_height")]
    public static extern double GetHeight(int terrain, double x, double y);

    [DllImport("hopper", EntryPoint = "get_height_derivatives")]
    public static extern void GetHeightDerivatives(int terrain, double x, double y, out double dx, out double dy);


    /* Session */

    public class Model
    {
        public Vector3[] nominalStance;
        public Vector3 maxDeviation;

        public double mass;
        public int eeCount;
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

        public Dim3D[] boundsFinalLinearPosition = { Dim3D.X, Dim3D.Z };
        public Dim3D[] boundsFinalLinearVelocity = { Dim3D.X, Dim3D.Y, Dim3D.Z };
        public Dim3D[] boundsFinalAngularPosition = { Dim3D.X, Dim3D.Y, Dim3D.Z };
        public Dim3D[] boundsFinalAngularVelocity = { Dim3D.X, Dim3D.Y, Dim3D.Z };

        public void UpdateAngles()
        {
            finalBaseAngularPosition.x = NearestAngle(initialBaseAngularPosition.x, finalBaseAngularPosition.x);
            finalBaseAngularPosition.y = NearestAngle(initialBaseAngularPosition.y, finalBaseAngularPosition.y);
            finalBaseAngularPosition.z = NearestAngle(initialBaseAngularPosition.z, finalBaseAngularPosition.z);
        }
    }

    public class Options
    {
        public float maxCpuTime = 20.0f;
        public int maxIter;
        public bool optimizePhaseDurations = false;
    }

    public class PathPoint
    {
        public float time;
        public Vector3 linear, angular;
    }

    public struct State
    {
        public Vector3 baseLinearPosition;
        public Vector3 baseLinearVelocity;
        public Vector3 baseAngularPosition;
        public Vector3 baseAngularVelocity;

        public Vector3[] eeMotions;
        public Vector3[] eeForces;
        public bool[] contacts;
    }

    public static Model GetModel(int session)
    {
        var m = new CHopper.Model();
        GetModel(session, out m);

        var model = new Model();

        model.mass = m.mass;
        model.eeCount = m.eeCount;

        model.nominalStance = new Vector3[m.eeCount];
        for (int id = 0; id < m.eeCount; ++id)
        {
            var array = new double[3];
            Array.Copy(m.nominalStance, id * 3, array, 0, array.Length);
            model.nominalStance[id] = LinearArrayToVector3(array);
        }

        model.maxDeviation = LinearArrayToVector3(m.maxDeviation);
        model.maxDeviation.x = Mathf.Abs(model.maxDeviation.x);
        model.maxDeviation.y = Mathf.Abs(model.maxDeviation.y);
        model.maxDeviation.z = Mathf.Abs(model.maxDeviation.z);

        return model;
    }

    public static void SetParams(int session, Parameters parameters)
    {
        parameters.UpdateAngles();

        var p = new CHopper.Parameters();

        p.initialBaseLinearPosition = LinearVector3ToArray(parameters.initialBaseLinearPosition);
        p.initialBaseLinearVelocity = LinearVector3ToArray(parameters.initialBaseLinearVelocity);
        p.initialBaseAngularPosition = AngularVector3ToArray(parameters.initialBaseAngularPosition);
        p.initialBaseAngularVelocity = AngularVector3ToArray(parameters.initialBaseAngularVelocity);

        p.finalBaseLinearPosition = LinearVector3ToArray(parameters.finalBaseLinearPosition);
        p.finalBaseLinearVelocity = LinearVector3ToArray(parameters.finalBaseLinearVelocity);
        p.finalBaseAngularPosition = AngularVector3ToArray(parameters.finalBaseAngularPosition);
        p.finalBaseAngularVelocity = AngularVector3ToArray(parameters.finalBaseAngularVelocity);

        p.initialEEPositions = new double[12];
        for (int id = 0; id < GetEECount(session); ++id)
        {
            var array = LinearVector3ToArray(parameters.initialEEPositions[id]);
            Array.Copy(array, 0, p.initialEEPositions, id * 3, array.Length);
        }

        p.boundsFinalLinearPosition = ConvertBoundDims(parameters.boundsFinalLinearPosition);
        p.boundsFinalLinearVelocity = ConvertBoundDims(parameters.boundsFinalLinearVelocity);
        p.boundsFinalAngularPosition = ConvertBoundDims(parameters.boundsFinalAngularPosition);
        p.boundsFinalAngularVelocity = ConvertBoundDims(parameters.boundsFinalAngularVelocity);

        SetParams(session, p);
    }

    public static void SetOptions(int session, Options options)
    {
        var o = new CHopper.Options();
        o.maxCpuTime = options.maxCpuTime;
        o.maxIter = options.maxIter;
        o.optimizePhaseDurations = options.optimizePhaseDurations;
        SetOptions(session, o);
    }

    public static void PushPathPoint(int session, PathPoint pathPoint)
    {
        var p = new CHopper.PathPoint();
        p.time = pathPoint.time;
        p.linear = LinearVector3ToArray(pathPoint.linear);
        p.angular = AngularVector3ToArray(pathPoint.angular);
        PushPathPoint(session, p);
    }

    public static bool GetSolutionState(int session, float time, out State state)
    {
        var s = new CHopper.State();
        var result = GetSolutionState(session, time, out s);

        state = new State();
        if (!result) return false;

        state.baseLinearPosition = LinearArrayToVector3(s.baseLinearPosition);
        state.baseLinearVelocity = LinearArrayToVector3(s.baseLinearVelocity);
        state.baseAngularPosition = AngularArrayToVector3(s.baseAngularPosition);
        state.baseAngularVelocity = AngularArrayToVector3(s.baseAngularVelocity);

        var eeCount = GetEECount(session);
        state.eeMotions = new Vector3[eeCount];
        state.eeForces = new Vector3[eeCount];
        state.contacts = new bool[eeCount];

        for (int id = 0; id < eeCount; ++id)
        {
            var array = new double[3];

            Array.Copy(s.eeMotions, id * 3, array, 0, array.Length);
            state.eeMotions[id] = LinearArrayToVector3(array);

            Array.Copy(s.eeForces, id * 3, array, 0, array.Length);
            state.eeForces[id] = LinearArrayToVector3(array);

            state.contacts[id] = s.contacts[id];
        }

        return true;
    }


    /* Terrain */

    public static int CreateTerrain(Vector3 pos, uint x, uint y, float unitSize)
    {
        var posArray = LinearVector3ToArray(pos);
        return CreateTerrain(posArray[0], posArray[1], posArray[2], x, y, unitSize);
    }


    /* Util functions */

    static float NearestAngle(float a, float b)
    {
        float c = Mathf.Cos((b - a) * Mathf.Deg2Rad);
        float s = Mathf.Sin((b - a) * Mathf.Deg2Rad);
        float d = Mathf.Acos(c) * Mathf.Rad2Deg;
        return s < 0 ? a - d : a + d;
    }

    static double[] LinearVector3ToArray(Vector3 vec)
    {
        var array = new double[3];
        array[0] = vec.z;
        array[1] = -vec.x;
        array[2] = vec.y;
        return array;
    }

    static double[] AngularVector3ToArray(Vector3 vec)
    {
        var v = vec * Mathf.Deg2Rad;
        var array = new double[3];
        array[0] = -v.z;
        array[1] = v.x;
        array[2] = -v.y;
        return array;
    }

    static Vector3 LinearArrayToVector3(double[] array)
    {
        var vec = Vector3.zero;
        vec.x = -(float)array[1];
        vec.y = (float)array[2];
        vec.z = (float)array[0];
        return vec;
    }

    static Vector3 AngularArrayToVector3(double[] array)
    {
        var vec = Vector3.zero;
        vec.x = (float)array[1];
        vec.y = -(float)array[2];
        vec.z = -(float)array[0];
        return vec * Mathf.Rad2Deg;
    }

    static byte ConvertBoundDims(Dim3D[] dims)
    {
        byte result = 0;
        if (dims.Contains(Dim3D.Z)) result |= 1;
        if (dims.Contains(Dim3D.X)) result |= 2;
        if (dims.Contains(Dim3D.Y)) result |= 4;
        return result;
    }


    /* Object Oriented API */

    public class Session
    {
        public int session { get; private set; }

        public double duration { get; private set; }
        public bool optimized { get; private set; } = false;
        public bool ready { get => SolutionReady(session); }

        public Session(HopperAPI.Robot robot)
        {
            session = CreateSession(robot);
            Debug.Log($"Session {session} Created");
        }

        ~Session()
        {
            HopperAPI.EndSession(session);
            Debug.Log($"Session {session} Ended");
        }

        public HopperAPI.Model GetModel() => HopperAPI.GetModel(session);

        public void SetTerrain(HopperAPI.Terrain terrain) => HopperAPI.SetTerrain(session, terrain.terrain);
        public void SetParams(HopperAPI.Parameters parameters) => HopperAPI.SetParams(session, parameters);
        public void SetOptions(HopperAPI.Options options) => HopperAPI.SetOptions(session, options);
        public void SetDuration(float duration) => HopperAPI.SetDuration(session, duration);

        public void PushPathPoint(HopperAPI.PathPoint pathPoint) => HopperAPI.PushPathPoint(session, pathPoint);
        public void PushGait(HopperAPI.Gait gait) => HopperAPI.PushGait(session, gait);

        public void StartOptimization()
        {
            optimized = true;
            HopperAPI.StartOptimization(session);
        }

        public HopperAPI.State GetState(float time)
        {
            var state = new HopperAPI.State();
            HopperAPI.GetSolutionState(session, time, out state);
            return state;
        }
    }

    public class Terrain
    {
        public int terrain { get; private set; }

        public Vector3 origin { get; private set; }
        public Vector2 size { get; private set; }
        public float unitSize { get; private set; }

        uint x, y;

        public Terrain(Vector3 origin, uint x, uint y, float unitSize)
        {
            this.origin = origin;
            this.size = new Vector2(x * unitSize, y * unitSize);
            this.unitSize = unitSize;

            this.x = x;
            this.y = y;

            terrain = HopperAPI.CreateTerrain(origin, x, y, unitSize);
            Debug.Log($"Terrain {terrain} Created");
        }

        ~Terrain()
        {
            HopperAPI.EndTerrain(terrain);
            Debug.Log($"Terrain {terrain} Ended");
        }

        public void SetHeight(uint x, uint y, double height)
        {
            if (x > this.x || y > this.y) return;
            HopperAPI.SetHeight(terrain, x, y, height);
        }

        public double GetHeight(double x, double y) => HopperAPI.GetHeight(terrain, x, y);
        public Vector2 GetHeightDerivatives(double x, double y)
        {
            double dx, dy;
            HopperAPI.GetHeightDerivatives(terrain, x, y, out dx, out dy);
            return new Vector2((float)dx, (float)dy);
        }
    }
}