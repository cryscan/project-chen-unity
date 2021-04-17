using Unity.Kinematica;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

using SnapshotProvider = Unity.SnapshotDebugger.SnapshotProvider;
using Unity.SnapshotDebugger;

[RequireComponent(typeof(AbilityRunner))]
[RequireComponent(typeof(MovementController))]
public class ClimbingAbility : SnapshotProvider, IAbility
{
    enum Layer { Climb = 13 }

    public struct FrameCapture
    {
        public bool mountButton;
    }

    public ClimberAnimation climber;
    Quaternion climberRotation;

    [Snapshot] FrameCapture capture;

    PoseSet idleCandidates;
    PoseSet locomotionCandidates;
    Trajectory trajectory;

    MovementController controller;
    Kinematica kinematica;

    public override void OnEnable()
    {
        base.OnEnable();

        climberRotation = climber.transform.rotation;

        controller = GetComponent<MovementController>();
        kinematica = GetComponent<Kinematica>();

        ref var synthesizer = ref kinematica.Synthesizer.Ref;

        idleCandidates = synthesizer.Query.Where("Idle", Locomotion.Default).And(Idle.Default);
        locomotionCandidates = synthesizer.Query.Where("Locomotion", Locomotion.Default).Except(Idle.Default);
        trajectory = synthesizer.CreateTrajectory(Allocator.Persistent);
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

        if (!rewind)
        {
            capture.mountButton = Input.GetButton("A Button");
        }
    }

    public IAbility OnUpdate(float deltaTime)
    {
        // controller.collisionEnabled = false;
        controller.groundSnap = false;
        controller.resolveGroundPenetration = false;
        controller.gravityEnabled = false;

        /*
        if (capture.mountButton)
        {
            LocomotionJob job = new LocomotionJob()
            {
                synthesizer = kinematica.Synthesizer,
                idleCandidates = idleCandidates,
                locomotionCandidates = locomotionCandidates,
                trajectory = trajectory,
                idle = true,
                maxPoseDerivation = 0.15f,
                responsiveness = 0.6f,
                minTrajectoryDeviation = 0.05f,
            };

            kinematica.AddJobDependency(job.Schedule());

            return this;
        }
        */

        ref var synthesizer = ref kinematica.Synthesizer.Ref;
        var position = climber.transform.position + climber.transform.up;
        var rotation = climber.transform.rotation * Quaternion.Inverse(climberRotation);
        AffineTransform transform = new AffineTransform(position, rotation);
        synthesizer.SetWorldTransform(transform);

        return capture.mountButton ? this : null;
    }

    public bool OnContact(ref MotionSynthesizer synthesizer, AffineTransform contactTransform, float deltaTime)
    {
        if (capture.mountButton)
        {
            ref var closure = ref controller.current;
            var collider = closure.collider as BoxCollider;
            if (collider != null && collider.gameObject.layer == (int)Layer.Climb)
            {
                var position = closure.colliderContactPoint;
                var rotation = Quaternion.LookRotation(climber.transform.forward, closure.colliderContactNormal);
                climber.Move(position, rotation);

                return true;
            }
        }

        return false;
    }

    public bool OnDrop(ref MotionSynthesizer synthesizer, float deltaTime)
    {
        return false;
    }
}
