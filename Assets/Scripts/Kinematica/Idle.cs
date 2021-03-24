using System;
using Unity.Kinematica;

#if UNITY_EDITOR
using Unity.Kinematica.Editor;
#endif

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