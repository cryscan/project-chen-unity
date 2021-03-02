using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public static class HopperBackend
{
    [DllImport("hopper", EntryPoint = "create_session")]
    public static extern int CreateSession(double duration = 2.0);

    [DllImport("hopper", EntryPoint = "end_session")]
    public static extern void EndSession(int session);

    [DllImport("hopper", EntryPoint = "set_initial_base_linear_position")]
    static extern void SetInitialBaseLinearPosition(int session, double x, double y, double z);

    [DllImport("hopper", EntryPoint = "set_initial_base_linear_velocity")]
    static extern void SetInitialBaseLinearVelocity(int session, double x, double y, double z);

    [DllImport("hopper", EntryPoint = "set_initial_base_angular_position")]
    static extern void SetInitialBaseAngularPosition(int session, double x, double y, double z);

    [DllImport("hopper", EntryPoint = "set_initial_base_angular_velocity")]
    static extern void SetInitialBaseAngularVelocity(int session, double x, double y, double z);

    [DllImport("hopper", EntryPoint = "set_final_base_linear_position")]
    static extern void SetFinalBaseLinearPosition(int session, double x, double y, double z);

    [DllImport("hopper", EntryPoint = "set_final_base_linear_velocity")]
    static extern void SetFinalBaseLinearVelocity(int session, double x, double y, double z);

    [DllImport("hopper", EntryPoint = "set_final_base_angular_position")]
    static extern void SetFinalBaseAngularPosition(int session, double x, double y, double z);

    [DllImport("hopper", EntryPoint = "set_final_base_angular_velocity")]
    static extern void SetFinalBaseAngularVelocity(int session, double x, double y, double z);

    [DllImport("hopper", EntryPoint = "set_initial_ee_position")]
    static extern void SetInitialEEPosition(int session, int id, double x, double y);

    [DllImport("hopper", EntryPoint = "start_optimization")]
    public static extern void StartOptimization(int session);

    [DllImport("hopper", EntryPoint = "get_solution")]
    static extern bool GetSolution(int session, double time, IntPtr baseLinear, IntPtr baseAngular, IntPtr eeMotion, IntPtr eeForce, out bool contact);

    public static void SetInitialBaseLinearPosition(int session, Vector3 data) => SetInitialBaseLinearPosition(session, data.z, -data.x, data.y);
    public static void SetInitialBaseLinearVelocity(int session, Vector3 data) => SetInitialBaseLinearVelocity(session, data.z, -data.x, data.y);
    public static void SetInitialBaseAngularPosition(int session, Vector3 data) => SetInitialBaseAngularPosition(session, -data.z, data.x, -data.y);
    public static void SetInitialBaseAngularVelocity(int session, Vector3 data) => SetInitialBaseAngularVelocity(session, -data.z, data.x, -data.y);
    public static void SetFinalBaseLinearPosition(int session, Vector3 data) => SetFinalBaseLinearPosition(session, data.z, -data.x, data.y);
    public static void SetFinalBaseLinearVelocity(int session, Vector3 data) => SetFinalBaseLinearVelocity(session, data.z, -data.x, data.y);
    public static void SetFinalBaseAngularPosition(int session, Vector3 data) => SetFinalBaseAngularPosition(session, -data.z, data.x, -data.y);
    public static void SetFinalBaseAngularVelocity(int session, Vector3 data) => SetFinalBaseAngularVelocity(session, -data.z, data.x, -data.y);
    public static void SetInitialEEPosition(int session, int id, Vector3 data) => SetInitialEEPosition(session, id, data.z, -data.x);

    static Vector3 GetLinearVector(IntPtr ptr)
    {
        var array = new double[3];
        Marshal.Copy(ptr, array, 0, array.Length);
        return new Vector3(-(float)array[1], (float)array[2], (float)array[0]);
    }

    static Vector3 GetAngularVector(IntPtr ptr)
    {
        var array = new double[3];
        Marshal.Copy(ptr, array, 0, array.Length);
        return new Vector3((float)array[1], -(float)array[2], -(float)array[0]);
    }

    public static bool GetSolution(int session, double time, out Vector3 baseLinear, out Vector3 baseAngular, out Vector3 eeMotion, out Vector3 eeForce, out bool contact)
    {
        var ps = (new IntPtr[4]).Select(x => Marshal.AllocHGlobal(sizeof(double) * 3)).ToArray();
        var result = GetSolution(session, time, ps[0], ps[1], ps[2], ps[3], out contact);

        if (result)
        {
            baseLinear = GetLinearVector(ps[0]);
            baseAngular = GetAngularVector(ps[1]);
            eeMotion = GetLinearVector(ps[2]);
            eeForce = GetLinearVector(ps[3]);
        }
        else
            baseLinear = baseAngular = eeMotion = eeForce = Vector3.zero;

        foreach (var p in ps) Marshal.FreeHGlobal(p);
        return result;
    }
}
