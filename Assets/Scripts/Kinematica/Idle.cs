using Unity.Kinematica;

#if UNITY_EDITOR
using System;
using Unity.Kinematica.Editor;

[Serializable]
[Tag("Idle", "#0ab266")]
internal struct IdleTag : Payload<Idle>
{
    public Idle Build(PayloadBuilder builder)
    {
        return Idle.Default;
    }
}
#endif

[Trait]
public struct Idle
{
    public static Idle Default => new Idle();
}