using System;
using Unity.Kinematica;
using Unity.Kinematica.Editor;

namespace BipedLocomotion
{
    [Trait]
    public struct Idle
    {
        public static Idle Default => new Idle();
    }

#if UNITY_EDITOR
    [Serializable]
    [Tag("Idle", "#4850d2")]
    internal struct IdleTag : Payload<Idle>
    {
        public Idle Build(PayloadBuilder builder)
        {
            return Idle.Default;
        }
    }
#endif
}