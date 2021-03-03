using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public static class HopperBackend
{
    [DllImport("hopper", EntryPoint = "create_session")]
    public static extern int CreateSession();

    [DllImport("hopper", EntryPoint = "end_session")]
    public static extern void EndSession(int session);

    [StructLayout(LayoutKind.Sequential)]
    struct _Boundary
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

    [DllImport("hopper", EntryPoint = "set_boundary")]
    static extern void SetBoundary(int session, ref _Boundary boundary);

    public struct Boundary
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

        public double duration;
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

    public static void SetBoundary(int session, ref Boundary boundary)
    {
        var _boundary = new _Boundary();

        _boundary.initialBaseLinearPosition = LinearVector3ToArray(boundary.initialBaseLinearPosition);
        _boundary.initialBaseLinearVelocity = LinearVector3ToArray(boundary.initialBaseLinearVelocity);
        _boundary.initialBaseAngularPosition = AngularVector3ToArray(boundary.initialBaseAngularPosition);
        _boundary.initialBaseAngularVelocity = AngularVector3ToArray(boundary.initialBaseAngularVelocity);

        _boundary.finalBaseLinearPosition = LinearVector3ToArray(boundary.finalBaseLinearPosition);
        _boundary.finalBaseLinearVelocity = LinearVector3ToArray(boundary.finalBaseLinearVelocity);
        _boundary.finalBaseAngularPosition = AngularVector3ToArray(boundary.finalBaseAngularPosition);
        _boundary.finalBaseAngularVelocity = AngularVector3ToArray(boundary.finalBaseAngularVelocity);

        _boundary.initialEEPosition = LinearVector3ToArray(boundary.initialEEPosition);
        _boundary.duration = boundary.duration;

        SetBoundary(session, ref _boundary);
    }

    [DllImport("hopper", EntryPoint = "start_optimization")]
    public static extern void StartOptimization(int session);

    [StructLayout(LayoutKind.Sequential)]
    struct _Solution
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
    static extern bool GetSolution(int session, double time, ref _Solution solution);

    public struct Solution
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

    public static bool GetSolution(int session, double time, out Solution solution)
    {
        var _solution = new _Solution();
        var result = GetSolution(session, time, ref _solution);

        solution = new Solution();
        if (!result) return false;

        solution.baseLinearPosition = LinearArrayToVector3(_solution.baseLinearPosition);
        solution.baseLinearVelocity = LinearArrayToVector3(_solution.baseLinearVelocity);
        solution.baseAngularPosition = AngularArrayToVector3(_solution.baseAngularPosition);
        solution.baseAngularVelocity = AngularArrayToVector3(_solution.baseAngularVelocity);
        solution.eeMotion = LinearArrayToVector3(_solution.eeMotion);
        solution.eeForce = LinearArrayToVector3(_solution.eeForce);
        solution.contact = _solution.contact;

        return true;
    }
}
