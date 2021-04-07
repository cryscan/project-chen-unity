using Unity.Burst;
using Unity.Kinematica;

#if UNITY_EDITOR
using System;
using Unity.Kinematica.Editor;

[Marker("Escape", "Brown")]
[Serializable]
public struct EscapeMarker : Payload<Escape>
{
    public Escape Build(PayloadBuilder builder)
    {
        return Escape.Default;
    }
}
#endif

[Trait]
public struct Escape
{
    public static Escape Default => new Escape();
}