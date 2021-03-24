using System;
using Unity.Kinematica;

#if UNITY_EDITOR
using Unity.Kinematica.Editor;
#endif

namespace BipedLocomotion
{
    [Trait]
    public struct Locomotion
    {
        public static Locomotion Default => new Locomotion();
    }

#if UNITY_EDITOR
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
}