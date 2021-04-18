using Unity.Kinematica;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

using SnapshotProvider = Unity.SnapshotDebugger.SnapshotProvider;
using Unity.SnapshotDebugger;

using static TagExtensions;

[RequireComponent(typeof(AbilityRunner))]
[RequireComponent(typeof(MovementController))]
public class ClimbingAbility : SnapshotProvider, IAbility
{
    public ClimberAnimation climber;

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
        public bool mountButton;
        public float3 movementDirection;
        public float moveIntensity;
    }

    public enum State
    {
        Mounting,
        Climbing,
    }

    public State state { get; private set; } = State.Mounting;
    Quaternion climberRotation;

    [Snapshot] FrameCapture capture;
    [Snapshot] AnchoredTransitionTask anchoredTransition;

    Kinematica kinematica;
    MovementController controller;

    public override void OnEnable()
    {
        base.OnEnable();

        climberRotation = climber.transform.rotation;

        kinematica = GetComponent<Kinematica>();
        controller = GetComponent<MovementController>();

        ref var synthesizer = ref kinematica.Synthesizer.Ref;
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnEarlyUpdate(bool rewind)
    {
        base.OnEarlyUpdate(rewind);

        if (!rewind)
        {
            capture.mountButton = Input.GetButton("A Button");
            Utility.GetInputMove(ref capture.movementDirection, ref capture.moveIntensity);
        }
    }

    public IAbility OnUpdate(float deltaTime)
    {
        controller.collisionEnabled = false;
        controller.groundSnap = false;
        controller.resolveGroundPenetration = false;
        controller.gravityEnabled = false;

        var active = anchoredTransition.valid;
        ref var synthesizer = ref kinematica.Synthesizer.Ref;

        if (active)
        {
            if (!anchoredTransition.IsComplete() && !anchoredTransition.IsFailed())
            {
                anchoredTransition.synthesizer = MemoryRef<MotionSynthesizer>.Create(ref synthesizer);
                kinematica.AddJobDependency(AnchoredTransitionJob.Schedule(ref anchoredTransition));

                return this;
            }
            else if (anchoredTransition.IsComplete())
            {
                ref var closure = ref controller.current;
                var position = closure.colliderContactPoint;
                var rotation = Quaternion.LookRotation(climber.transform.forward, closure.colliderContactNormal);
                climber.Move(position, rotation);
            }

            anchoredTransition.Dispose();
            anchoredTransition = AnchoredTransitionTask.Invalid;
            return anchoredTransition.IsComplete() ? this : null;
        }
        else
        {
            state = State.Climbing;

            var position = climber.transform.position + climber.transform.up;
            var rotation = climber.transform.rotation * Quaternion.Inverse(climberRotation);
            AffineTransform transform = new AffineTransform(position, rotation);
            synthesizer.SetWorldTransform(transform);

            return capture.mountButton ? this : null;
        }
    }

    public bool OnContact(ref MotionSynthesizer synthesizer, AffineTransform contactTransform, float deltaTime)
    {
        if (capture.mountButton)
        {
            var collider = controller.current.collider;
            var type = Parkour.Create(collider.gameObject.layer);
            if (type.IsType(Parkour.Type.Climb))
            {
                if (IsAxis(collider, contactTransform, Missing.forward) ||
                    IsAxis(collider, contactTransform, Missing.right))
                {
                    OnContactDebug(ref synthesizer, contactTransform, type);

                    ref Binary binary = ref synthesizer.Binary;

                    var sequence = GetPoseSequence(ref binary, contactTransform, type, contactThreshold, false);

                    anchoredTransition.Dispose();
                    anchoredTransition = AnchoredTransitionTask.Create(ref synthesizer, sequence, contactTransform, capture.movementDirection, maximumLinearError, maximumAngularError);

                    state = State.Mounting;
                    return true;
                }
            }
        }

        return false;
    }

    public bool OnDrop(ref MotionSynthesizer synthesizer, float deltaTime)
    {
        return false;
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
