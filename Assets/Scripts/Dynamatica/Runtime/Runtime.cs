using System.Runtime.InteropServices;

namespace Dynamatica.Runtime
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
    public class Options
    {
        public double maxCpuTime;
        public int maxIter;
        public bool optimizePhaseDurations;
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
    public struct State
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseLinearPosition;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseLinearVelocity;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseAngularPosition;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public double[] baseAngularVelocity;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] eeMotions;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public double[] eeForces;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public bool[] contacts;
    }

    public static class Core
    {
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

        public static Dim3D ToLinear(this Dim6D dim) => (Dim3D)((byte)dim >> 3);
        public static Dim6D ToLinear(this Dim3D dim) => (Dim6D)((byte)dim << 3);

        public static Dim3D ToAngular(this Dim6D dim) => (Dim3D)((byte)dim & 0x7);
        public static Dim6D ToAngular(this Dim3D dim) => (Dim6D)((byte)dim);
    }
}