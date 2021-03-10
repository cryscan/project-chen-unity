using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public static class HopperAPI
{
    [System.Serializable]
    public enum RobotModel { Monoped, Biped, Hyq, Anymal }

    [DllImport("hopper", EntryPoint = "create_session")]
    public static extern int CreateSession(RobotModel model = 0);

    [DllImport("hopper", EntryPoint = "end_session")]
    public static extern void EndSession(int session);

    [StructLayout(LayoutKind.Sequential)]
    struct _ModelInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] nominalStance;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] maxDeviation;
    }

    [DllImport("hopper", EntryPoint = "get_model_info")]
    static extern void GetModelInfo(int session, ref _ModelInfo modelInfo);

    [DllImport("hopper", EntryPoint = "get_ee_count")]
    public static extern int GetEECount(int session);

    public struct ModelInfo
    {
        public Vector3[] nominalStance;
        public Vector3 maxDeviation;
        public int eeCount;
    }

    public static ModelInfo GetModelInfo(int session)
    {
        var _modelInfo = new _ModelInfo();
        GetModelInfo(session, ref _modelInfo);

        var modelInfo = new ModelInfo();
        var eeCount = GetEECount(session);

        modelInfo.eeCount = eeCount;
        modelInfo.nominalStance = new Vector3[eeCount];
        for (int id = 0; id < eeCount; ++id)
        {
            var array = new double[3];
            Array.Copy(_modelInfo.nominalStance, id * 3, array, 0, array.Length);
            modelInfo.nominalStance[id] = LinearArrayToVector3(array);
        }

        modelInfo.maxDeviation = LinearArrayToVector3(_modelInfo.maxDeviation);
        modelInfo.maxDeviation.x = Mathf.Abs(modelInfo.maxDeviation.x);
        modelInfo.maxDeviation.y = Mathf.Abs(modelInfo.maxDeviation.y);
        modelInfo.maxDeviation.z = Mathf.Abs(modelInfo.maxDeviation.z);

        return modelInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct _Bound
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

        public double duration;

        public double maxCpuTime;
        public int maxIter;
        public bool optimizePhaseDurations;
    }

    [DllImport("hopper", EntryPoint = "set_bound")]
    static extern void SetBound(int session, ref _Bound bound);

    public class Bound
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

        public float duration = 2.4f;

        public float maxCpuTime = 20.0f;
        public int maxIter;
        public bool optimizePhaseDurations = false;

        static float NearestAngle(float a, float b)
        {
            float c = Mathf.Cos((b - a) * Mathf.Deg2Rad);
            float s = Mathf.Sin((b - a) * Mathf.Deg2Rad);
            float d = Mathf.Acos(c) * Mathf.Rad2Deg;
            return s < 0 ? a - d : a + d;
        }

        public void CorrectAngles()
        {
            finalBaseAngularPosition.x = NearestAngle(initialBaseAngularPosition.x, finalBaseAngularPosition.x);
            finalBaseAngularPosition.y = NearestAngle(initialBaseAngularPosition.y, finalBaseAngularPosition.y);
            finalBaseAngularPosition.z = NearestAngle(initialBaseAngularPosition.z, finalBaseAngularPosition.z);
        }
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

    public static void SetBound(int session, Bound bound)
    {
        bound.CorrectAngles();

        var _bound = new _Bound();

        _bound.initialBaseLinearPosition = LinearVector3ToArray(bound.initialBaseLinearPosition);
        _bound.initialBaseLinearVelocity = LinearVector3ToArray(bound.initialBaseLinearVelocity);
        _bound.initialBaseAngularPosition = AngularVector3ToArray(bound.initialBaseAngularPosition);
        _bound.initialBaseAngularVelocity = AngularVector3ToArray(bound.initialBaseAngularVelocity);

        _bound.finalBaseLinearPosition = LinearVector3ToArray(bound.finalBaseLinearPosition);
        _bound.finalBaseLinearVelocity = LinearVector3ToArray(bound.finalBaseLinearVelocity);
        _bound.finalBaseAngularPosition = AngularVector3ToArray(bound.finalBaseAngularPosition);
        _bound.finalBaseAngularVelocity = AngularVector3ToArray(bound.finalBaseAngularVelocity);

        _bound.initialEEPositions = new double[12];
        for (int id = 0; id < GetEECount(session); ++id)
        {
            var array = LinearVector3ToArray(bound.initialEEPositions[id]);
            Array.Copy(array, 0, _bound.initialEEPositions, id * 3, array.Length);
        }
        _bound.duration = bound.duration;

        _bound.maxCpuTime = bound.maxCpuTime;
        _bound.maxIter = bound.maxIter;
        _bound.optimizePhaseDurations = bound.optimizePhaseDurations;

        SetBound(session, ref _bound);
    }

    [DllImport("hopper", EntryPoint = "start_optimization")]
    public static extern void StartOptimization(int session);

    [StructLayout(LayoutKind.Sequential)]
    struct _State
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseLinearPosition;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseLinearVelocity;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseAngularPosition;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseAngularVelocity;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] eeMotions;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] eeForces;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public bool[] contacts;
    }

    [DllImport("hopper", EntryPoint = "get_solution")]
    static extern bool GetSolution(int session, double time, ref _State state);

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

    public static bool GetSolution(int session, double time, out State state)
    {
        var _state = new _State();
        var result = GetSolution(session, time, ref _state);

        state = new State();
        if (!result) return false;

        state.baseLinearPosition = LinearArrayToVector3(_state.baseLinearPosition);
        state.baseLinearVelocity = LinearArrayToVector3(_state.baseLinearVelocity);
        state.baseAngularPosition = AngularArrayToVector3(_state.baseAngularPosition);
        state.baseAngularVelocity = AngularArrayToVector3(_state.baseAngularVelocity);

        var eeCount = GetEECount(session);
        state.eeMotions = new Vector3[eeCount];
        state.eeForces = new Vector3[eeCount];
        state.contacts = new bool[eeCount];

        for (int id = 0; id < eeCount; ++id)
        {
            var array = new double[3];

            Array.Copy(_state.eeMotions, id * 3, array, 0, array.Length);
            state.eeMotions[id] = LinearArrayToVector3(array);

            Array.Copy(_state.eeForces, id * 3, array, 0, array.Length);
            state.eeForces[id] = LinearArrayToVector3(array);

            state.contacts[id] = _state.contacts[id];
        }

        return true;
    }
}
