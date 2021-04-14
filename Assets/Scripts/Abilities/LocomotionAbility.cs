using UnityEngine;
using Unity.Kinematica;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.SnapshotDebugger;

[BurstCompile]
public struct LocomotionJob : IJob
{
    public MemoryRef<MotionSynthesizer> synthesizer;
    public PoseSet idleCandidates;
    public PoseSet locomotionCandidates;
    public Trajectory trajectory;

    public bool idle;
    public float maxPoseDerivation;
    public float responsiveness;
    public float minTrajectoryDeviation;

    ref MotionSynthesizer Synthesizer => ref synthesizer.Ref;

    public void Execute()
    {
        if (idle && Synthesizer.MatchPose(idleCandidates, Synthesizer.Time, MatchOptions.DontMatchIfCandidateIsPlaying | MatchOptions.LoopSegment, maxPoseDerivation))
        {
            return;
        }

        Synthesizer.MatchPoseAndTrajectory(locomotionCandidates, Synthesizer.Time, trajectory, MatchOptions.None, responsiveness, minTrajectoryDeviation);
    }
}

[RequireComponent(typeof(AbilityRunner))]
[RequireComponent(typeof(MovementController))]
public class LocomotionAbility : SnapshotProvider, IAbility, IAbilityAnimatorMove
{
    [Header("Prediction settings")]
    [Tooltip("Desired speed in meters per second for slow movement.")]
    [Range(0.0f, 10.0f)]
    public float desiredSpeed = 3;

    [Tooltip("How fast or slow the target velocity is supposed to be reached.")]
    [Range(0.0f, 1.0f)]
    public float velocityPercentage = 0.1f;

    [Tooltip("How fast or slow the desired forward direction is supposed to be reached.")]
    [Range(0.0f, 1.0f)]
    public float forwardPercentage = 0.1f;

    [Tooltip("Relative weighting for pose and trajectory matching.")]
    [Range(0.0f, 1.0f)]
    public float responsiveness = 0.6f;

    [Tooltip("Speed in meters per second at which the character is considered to be braking (assuming player release the stick).")]
    [Range(0.0f, 10.0f)]
    public float brakingSpeed = 0.4f;

    [Header("Motion correction")]
    [Tooltip("How much root motion distance should be corrected to match desired trajectory.")]
    [Range(0.0f, 1.0f)]
    public float correctTranslationPercentage = 0.0f;

    [Tooltip("How much root motion rotation should be corrected to match desired trajectory.")]
    [Range(0.0f, 1.0f)]
    public float correctRotationPercentage = 1.0f;

    [Tooltip("Minimum character move speed (m/s) before root motion correction is applied.")]
    [Range(0.0f, 10.0f)]
    public float correctMotionStartSpeed = 1.0f;

    [Tooltip("Character move speed (m/s) at which root motion correction is fully effective.")]
    [Range(0.0f, 10.0f)]
    public float correctMotionEndSpeed = 2.0f;

    MovementController controller;
    IAbility[] abilities;

    Kinematica kinematica;

    PoseSet idleCandidates;
    PoseSet locomotionCandidates;
    Trajectory trajectory;

    [Snapshot] float3 movementDirection = Missing.forward;
    [Snapshot] float moveIntensity = 0.0f;
    [Snapshot] bool jumpButton;

    [Snapshot] float3 rootVelocity = float3.zero;

    float desiredLinearSpeed => desiredSpeed;

    public override void OnEnable()
    {
        base.OnEnable();

        controller = GetComponent<MovementController>();
        abilities = GetComponents<IAbility>();

        kinematica = GetComponent<Kinematica>();
        ref var synthesizer = ref kinematica.Synthesizer.Ref;

        idleCandidates = synthesizer.Query.Where("Idle", Locomotion.Default).And(Idle.Default);
        locomotionCandidates = synthesizer.Query.Where("Locomotion", Locomotion.Default).Except(Idle.Default);
        trajectory = synthesizer.CreateTrajectory(Allocator.Persistent);

        synthesizer.PlayFirstSequence(idleCandidates);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        idleCandidates.Dispose();
        locomotionCandidates.Dispose();
        trajectory.Dispose();
    }

    public override void OnEarlyUpdate(bool rewind)
    {
        base.OnEarlyUpdate(rewind);

        jumpButton = Input.GetButton("A Button");
        Utility.GetInputMove(ref movementDirection, ref moveIntensity);
    }

    public IAbility OnUpdate(float deltaTime)
    {
        ref var synthesizer = ref kinematica.Synthesizer.Ref;

        controller.collisionEnabled = true;
        controller.groundSnap = true;
        controller.resolveGroundPenetration = true;
        controller.gravityEnabled = true;

        float desiredSpeed = moveIntensity * desiredLinearSpeed;
        var currentSpeed = math.length(synthesizer.CurrentVelocity);
        bool idle = moveIntensity == 0;

        float maxPoseDerivation = 0.15f;
        float minTrajectoryDeviation = 0.05f;

        if (!idle && currentSpeed < brakingSpeed) minTrajectoryDeviation = 0.08f;

        var prediction = TrajectoryPrediction.CreateFromDirection(ref kinematica.Synthesizer.Ref,
            movementDirection,
            desiredSpeed,
            trajectory,
            velocityPercentage,
            forwardPercentage);

        controller.Snapshot();
        var transform = prediction.Transform;
        var worldRootTransform = synthesizer.WorldRootTransform;
        var inverseSampleRate = Missing.recip(synthesizer.Binary.SampleRate);

        bool canTransite = true;
        IAbility contactAbility = null;

        while (prediction.Push(transform))
        {
            transform = prediction.Advance;

            controller.MoveTo(worldRootTransform.transform(transform.t));
            controller.Tick(inverseSampleRate);

            ref var closure = ref controller.current;
            if (closure.isColliding && canTransite)
            {
                float3 contactPoint = closure.colliderContactPoint;
                contactPoint.y = controller.Position.y;

                float3 contactNormal = closure.colliderContactNormal;
                quaternion q = math.mul(transform.q, Missing.forRotation(Missing.zaxis(transform.q), contactNormal));

                Debug.DrawRay(contactPoint, contactNormal, Color.black);
                AffineTransform contactTransform = new AffineTransform(contactPoint, q);

                // Truncate trajectory when facing obstacle.
                var direction = worldRootTransform.transformDirection(Missing.zaxis(transform.q));
                var projectedSpeed = math.length(direction - Missing.project(direction, contactNormal)) * desiredSpeed;
                if (projectedSpeed < brakingSpeed) break;

                if (contactAbility == null)
                {
                    foreach (var ability in abilities)
                    {
                        if (ability.OnContact(ref synthesizer, contactTransform, deltaTime))
                        {
                            contactAbility = ability;
                            break;
                        }
                    }
                }
                canTransite = false;
            }
            else if (closure.isColliding && !canTransite)
            {
                var contactNormal = worldRootTransform.inverse().transformDirection(closure.colliderContactNormal);
                var direction = Missing.zaxis(transform.q);
                var correctDirection = direction - Missing.project(direction, contactNormal);
                transform.q = math.mul(Missing.forRotation(direction, correctDirection), transform.q);
            }
            else if (!closure.isGrounded)
            {
                if (contactAbility == null)
                {
                    foreach (var ability in abilities)
                    {
                        if (ability.OnDrop(ref synthesizer, deltaTime))
                        {
                            contactAbility = ability;
                            break;
                        }
                    }
                }

                // Truncate trajectory when going to drop.
                if (!jumpButton) break;
            }

            transform.t = worldRootTransform.inverseTransform(controller.Position);
            prediction.Transform = transform;

            Debug.DrawRay(controller.Position, Vector3.up, Color.green);
        }

        controller.Rewind();

        LocomotionJob job = new LocomotionJob()
        {
            synthesizer = kinematica.Synthesizer,
            idleCandidates = idleCandidates,
            locomotionCandidates = locomotionCandidates,
            trajectory = trajectory,
            idle = idle,
            maxPoseDerivation = maxPoseDerivation,
            responsiveness = responsiveness,
            minTrajectoryDeviation = minTrajectoryDeviation,
        };

        kinematica.AddJobDependency(job.Schedule());

        if (contactAbility != null) return contactAbility;
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

    public void OnAbilityAnimatorMove()
    {
        var kinematica = GetComponent<Kinematica>();
        if (kinematica.Synthesizer.IsValid)
        {
            ref MotionSynthesizer synthesizer = ref kinematica.Synthesizer.Ref;

            AffineTransform rootMotion = synthesizer.SteerRootMotion(trajectory, correctTranslationPercentage, correctRotationPercentage, correctMotionStartSpeed, correctMotionEndSpeed);
            AffineTransform rootTransform = AffineTransform.Create(transform.position, transform.rotation) * rootMotion;

            synthesizer.SetWorldTransform(AffineTransform.Create(rootTransform.t, rootTransform.q), true);

            if (synthesizer.deltaTime >= 0.0f)
            {
                rootVelocity = rootMotion.t / synthesizer.deltaTime;
            }
        }
    }

    void OnGUI()
    {
        InputUtility.DisplayMissingInputs(InputUtility.ActionButtonInput | InputUtility.MoveInput | InputUtility.CameraInput);
    }
}
