using Unity.Kinematica;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using static TagExtensions;

using SnapshotProvider = Unity.SnapshotDebugger.SnapshotProvider;
using Unity.SnapshotDebugger;

public class ClimbingAbility : SnapshotProvider, IAbility
{
    public IAbility OnUpdate(float deltaTime)
    {
        return this;
    }

    public bool OnContact(ref MotionSynthesizer synthesizer, AffineTransform contactTransform, float deltaTime)
    {
        return false;
    }

    public bool OnDrop(ref MotionSynthesizer synthesizer, float deltaTime)
    {
        return false;
    }
}
