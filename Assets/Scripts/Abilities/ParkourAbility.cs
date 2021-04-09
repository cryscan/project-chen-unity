using UnityEngine;
using Unity.Kinematica;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.SnapshotDebugger;

using static TagExtensions;

[RequireComponent(typeof(AbilityRunner))]
[RequireComponent(typeof(MovementController))]
public class ParkourAbility : SnapshotProvider, IAbility
{
    [Header("Transition settings")]
    [Tooltip("Distance in meters for performing movement validity checks.")]
    [Range(0.0f, 1.0f)]
    public float contactThreshold;

    [Tooltip("Maximum linear error for transition poses.")]
    [Range(0.0f, 1.0f)]
    public float maximumLinearError;

    [Tooltip("Maximum angular error for transition poses.")]
    [Range(0.0f, 180.0f)]
    public float maximumAngularError;

    [Header("Debug settings")]
    [Tooltip("Enables debug display for this ability.")]
    public bool enableDebugging;

    [Tooltip("Determines the movement to debug.")]
    public int debugIndex;

    [Tooltip("Controls the pose debug display.")]
    [Range(0, 100)]
    public int debugPoseIndex;

    public struct FrameCapture
    {
        public bool jumpButton;
        public float3 movementDirection;
        public float moveIntensity;
    }

    [Snapshot]
    FrameCapture capture;

    [Snapshot]
    AnchoredTransitionTask anchoredTransition;

    Kinematica kinematica;
    MovementController controller;

    public override void OnEnable()
    {
        base.OnEnable();

        anchoredTransition = AnchoredTransitionTask.Invalid;

        controller = GetComponent<MovementController>();
        kinematica = GetComponent<Kinematica>();
    }

    public override void OnDisable()
    {
        base.OnDisable();

        anchoredTransition.Dispose();
    }

    public override void OnEarlyUpdate(bool rewind)
    {
        base.OnEarlyUpdate(rewind);

        if (!rewind)
        {
            capture.jumpButton = Input.GetButton("A Button");
            Utility.GetInputMove(ref capture.movementDirection, ref capture.moveIntensity);
        }
    }

    public IAbility OnUpdate(float deltaTime)
    {
        bool active = anchoredTransition.valid;

        controller.collisionEnabled = !active;
        controller.groundSnap = !active;
        controller.resolveGroundPenetration = !active;
        controller.gravityEnabled = !active;

        if (active)
        {
            ref var synthesizer = ref kinematica.Synthesizer.Ref;

            if (!anchoredTransition.IsState(AnchoredTransitionTask.State.Complete) && !anchoredTransition.IsState(AnchoredTransitionTask.State.Failed))
            {
                anchoredTransition.synthesizer = MemoryRef<MotionSynthesizer>.Create(ref synthesizer);
                kinematica.AddJobDependency(AnchoredTransitionJob.Schedule(ref anchoredTransition));

                return this;
            }

            anchoredTransition.Dispose();
            anchoredTransition = AnchoredTransitionTask.Invalid;
        }

        return null;
    }

    public bool OnContact(ref MotionSynthesizer synthesizer, AffineTransform contactTransform, float deltaTime)
    {
        if (capture.jumpButton)
        {
            ref var closure = ref controller.current;

            var collider = closure.collider;

            int layerMask = 1 << collider.gameObject.layer;

            var type = Parkour.Create(collider.gameObject.layer);

            if (type.IsType(Parkour.Type.Wall) || type.IsType(Parkour.Type.Table))
            {
                if (IsAxis(collider, contactTransform, Missing.forward))
                {
                    return OnContact(ref synthesizer, contactTransform, deltaTime, type);
                }
            }
            else if (type.IsType(Parkour.Type.Platform))
            {
                if (IsAxis(collider, contactTransform, Missing.forward) ||
                    IsAxis(collider, contactTransform, Missing.right))
                {
                    return OnContact(ref synthesizer, contactTransform, deltaTime, type);
                }
            }
            else if (type.IsType(Parkour.Type.Ledge))
            {
                if (IsAxis(collider, contactTransform, Missing.right))
                {
                    return OnContact(ref synthesizer, contactTransform, deltaTime, type);
                }
            }
        }

        return false;
    }

    public bool OnDrop(ref MotionSynthesizer synthesizer, float deltaTime)
    {
        MovementController controller = GetComponent<MovementController>();

        if (controller.previous.isGrounded && controller.previous.ground != null)
        {
            Transform ground = controller.previous.ground;
            BoxCollider collider = ground.GetComponent<BoxCollider>();

            if (collider != null)
            {
                NativeArray<float3> vertices = new NativeArray<float3>(4, Allocator.Persistent);

                Vector3 center = collider.center;
                Vector3 size = collider.size;

                vertices[0] = ground.TransformPoint(center + new Vector3(-size.x, size.y, size.z) * 0.5f);
                vertices[1] = ground.TransformPoint(center + new Vector3(size.x, size.y, size.z) * 0.5f);
                vertices[2] = ground.TransformPoint(center + new Vector3(size.x, size.y, -size.z) * 0.5f);
                vertices[3] = ground.TransformPoint(center + new Vector3(-size.x, size.y, -size.z) * 0.5f);

                float3 p = controller.previous.position;
                AffineTransform contactTransform =
                    GetClosestTransform(vertices[0], vertices[1], p);
                float minimumDistance = math.length(contactTransform.t - p);

                for (int i = 1; i < 4; ++i)
                {
                    int j = (i + 1) % 4;
                    AffineTransform candidateTransform =
                        GetClosestTransform(vertices[i], vertices[j], p);
                    float distance = math.length(candidateTransform.t - p);
                    if (distance < minimumDistance)
                    {
                        minimumDistance = distance;
                        contactTransform = candidateTransform;
                    }
                }

                vertices.Dispose();

                return OnContact(ref synthesizer, contactTransform,
                    deltaTime, Parkour.Create(Parkour.Type.DropDown));
            }
        }

        return false;
    }

    bool OnContact(ref MotionSynthesizer synthesizer, AffineTransform contactTransform, float deltaTime, Parkour type)
    {
        OnContactDebug(ref synthesizer, contactTransform, type);

        ref Binary binary = ref synthesizer.Binary;

        var sequence = GetPoseSequence(ref binary, contactTransform, type, contactThreshold);

        anchoredTransition.Dispose();
        anchoredTransition = AnchoredTransitionTask.Create(ref synthesizer, sequence, contactTransform, capture.movementDirection, maximumLinearError, maximumAngularError);

        return true;
    }


    void OnContactDebug(ref MotionSynthesizer synthesizer, AffineTransform contactTransform, Parkour type)
    {
        if (enableDebugging)
        {
            DisplayTransition(ref synthesizer, contactTransform, type, contactThreshold);
        }
    }

    void DisplayTransition<T>(ref MotionSynthesizer synthesizer, AffineTransform contactTransform, T value, float contactThreshold) where T : struct
    {
        if (enableDebugging)
        {
            ref Binary binary = ref synthesizer.Binary;

            NativeArray<OBB> obbs = GetBoundsFromContactPoints(ref binary, contactTransform, value, contactThreshold);

            // Display all relevant box colliders
            int numObbs = obbs.Length;
            for (int i = 0; i < numObbs; ++i)
            {
                OBB obb = obbs[i];
                obb.transform = contactTransform * obb.transform;
                DebugDraw(obb, Color.cyan);
            }

            var tagTraitIndex = binary.GetTraitIndex(value);

            int numTags = binary.numTags;

            int validIndex = 0;

            for (int i = 0; i < numTags; ++i)
            {
                ref Binary.Tag tag = ref binary.GetTag(i);

                if (tag.traitIndex == tagTraitIndex)
                {
                    if (validIndex == debugIndex)
                    {
                        DebugDrawContacts(ref binary, ref tag,
                            contactTransform, obbs, contactThreshold);

                        DebugDrawPoseAndTrajectory(ref binary, ref tag,
                            contactTransform, debugPoseIndex);

                        return;
                    }

                    validIndex++;
                }
            }

            obbs.Dispose();
        }
    }
}
