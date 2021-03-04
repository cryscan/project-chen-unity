using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public static class HopperAPI
{
    [DllImport("hopper", EntryPoint = "create_session")]
    public static extern int CreateSession();

    [DllImport("hopper", EntryPoint = "end_session")]
    public static extern void EndSession(int session);

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

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] initialEEPosition;

        public double duration;
    }

    [DllImport("hopper", EntryPoint = "set_bound")]
    static extern void SetBoundary(int session, ref _Bound boundary);

    public struct Bound
    {
        public Vector3 initialBaseLinearPosition;
        public Vector3 initialBaseLinearVelocity;
        public Vector3 initialBaseAngularPosition;
        public Vector3 initialBaseAngularVelocity;

        public Vector3 finalBaseLinearPosition;
        public Vector3 finalBaseLinearVelocity;
        public Vector3 finalBaseAngularPosition;
        public Vector3 finalBaseAngularVelocity;

        public Vector3 initialEEPosition;

        public float duration;

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

    public static void SetBound(int session, ref Bound bound)
    {
        bound.CorrectAngles();

        var _boundary = new _Bound();

        _boundary.initialBaseLinearPosition = LinearVector3ToArray(bound.initialBaseLinearPosition);
        _boundary.initialBaseLinearVelocity = LinearVector3ToArray(bound.initialBaseLinearVelocity);
        _boundary.initialBaseAngularPosition = AngularVector3ToArray(bound.initialBaseAngularPosition);
        _boundary.initialBaseAngularVelocity = AngularVector3ToArray(bound.initialBaseAngularVelocity);

        _boundary.finalBaseLinearPosition = LinearVector3ToArray(bound.finalBaseLinearPosition);
        _boundary.finalBaseLinearVelocity = LinearVector3ToArray(bound.finalBaseLinearVelocity);
        _boundary.finalBaseAngularPosition = AngularVector3ToArray(bound.finalBaseAngularPosition);
        _boundary.finalBaseAngularVelocity = AngularVector3ToArray(bound.finalBaseAngularVelocity);

        _boundary.initialEEPosition = LinearVector3ToArray(bound.initialEEPosition);
        _boundary.duration = bound.duration;

        SetBoundary(session, ref _boundary);
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

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] eeMotion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] eeForce;
        public bool contact;
    }

    [DllImport("hopper", EntryPoint = "get_solution")]
    static extern bool GetSolution(int session, double time, ref _State state);

    public struct State
    {
        public Vector3 baseLinearPosition;
        public Vector3 baseLinearVelocity;
        public Vector3 baseAngularPosition;
        public Vector3 baseAngularVelocity;

        public Vector3 eeMotion;
        public Vector3 eeForce;
        public bool contact;
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
        state.eeMotion = LinearArrayToVector3(_state.eeMotion);
        state.eeForce = LinearArrayToVector3(_state.eeForce);
        state.contact = _state.contact;

        return true;
    }
}
