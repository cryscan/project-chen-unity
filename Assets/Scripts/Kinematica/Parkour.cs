using Unity.Kinematica;

#if UNITY_EDITOR
using System;
using Unity.Kinematica.Editor;

[Serializable]
[Tag("Parkour", "#5048d2")]
public struct ParkourTag : Payload<Parkour>
{
    public Parkour.Type type;

    public Parkour Build(PayloadBuilder builder)
    {
        return Parkour.Create(type);
    }

    public static ParkourTag Create(Parkour.Type type)
    {
        return new ParkourTag
        {
            type = type
        };
    }
}
#endif

[Trait]
public struct Parkour
{
    public enum Type
    {
        Wall = 9,
        Ledge = 10,
        Table = 11,
        Platform = 12,
        DropDown = 13
    }

    public Type type;

    public bool IsType(Type type)
    {
        return this.type == type;
    }

    public static Parkour Create(Type type)
    {
        return new Parkour
        {
            type = type
        };
    }

    public static Parkour Create(int layer)
    {
        return Create((Type)layer);
    }
}