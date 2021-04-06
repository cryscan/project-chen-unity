using Unity.Kinematica;
using Unity.Mathematics;

#if UNITY_EDITOR
using System;
using UnityEngine;
using Unity.Kinematica.Editor;

[Marker("Anchor", "Green")]
[Serializable]
public struct AnchorMarker : Payload<Anchor>
{
    [MoveManipulator]
    public Vector3 position;

    [RotateManipulator]
    public Quaternion rotation;

    public Anchor Build(PayloadBuilder builder)
    {
        return new Anchor
        {
            transform = new AffineTransform(position, rotation)
        };
    }
}
#endif

[Trait]
public struct Anchor
{
    // Transform relative to the trajectory transform
    // of the frame at which the anchor has been placed
    public AffineTransform transform;
}