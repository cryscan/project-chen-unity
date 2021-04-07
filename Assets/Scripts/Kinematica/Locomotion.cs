using Unity.Kinematica;

#if UNITY_EDITOR
using System;
using Unity.Kinematica.Editor;

[Serializable]
[Tag("Locomotion", "#4850d2")]
public struct LocomotionTag : Payload<Locomotion>
{
    public Locomotion Build(PayloadBuilder builder)
    {
        return Locomotion.Default;
    }
}
#endif

[Trait]
public struct Locomotion
{
    public static Locomotion Default => new Locomotion();
}