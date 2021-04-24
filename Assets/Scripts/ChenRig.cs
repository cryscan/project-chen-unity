using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ChenRig : MonoBehaviour
{
    enum State { Standing, Running, Parkouring, Climbing }
    enum Side { Left = 1, Right = -1 }

    [System.Serializable]
    public struct Rig
    {
        public Transform root;
        public Transform hip;
        public Transform torso;
        public Transform chest;
        public Transform shoulder;
        public Transform footLeft, footRight;
        public Transform handLeft, handRight;
        public Transform pivotLeft, pivotRight;
    }

    [System.Serializable]
    public struct Robot
    {
        public Transform root;
        public Transform body;
        public Transform footLeft, footRight;
    }

    [System.Serializable]
    public struct Climber
    {
        public Transform root;
        public Transform body;
        public Transform handLeft, handRight;
        public Transform footLeft, footRight;
    }

    [System.Serializable]
    public struct Hip
    {
        public float stance;
        public float locomotion;
        public float parkour;

        [HideInInspector] public float nominal;
        [HideInInspector] public float height;
    }

    struct Stance
    {
        public float nominal;

        public Quaternion rootRotation;
        public Quaternion hipRotation;
        public Quaternion torsoRotation;
        public Quaternion chestRotation;

        public Quaternion climberRotation;

        public Stance(Robot robot, Rig rig, Climber climber)
        {
            nominal = (robot.footLeft.localPosition.z + robot.footRight.localPosition.z) / 2;

            rootRotation = rig.root.rotation;
            hipRotation = rig.hip.localRotation;
            torsoRotation = rig.torso.localRotation;
            chestRotation = rig.chest.localRotation;

            climberRotation = climber.root.rotation;
        }
    }

    struct Snap
    {
        public bool valid;
        public Vector3 point;
        public Vector3 direction;
    }

    [SerializeField] AbilityRunner abilityRunner;

    [SerializeField] Rig rig;
    [SerializeField] Robot robot;
    [SerializeField] Climber climber;

    [SerializeField] Hip hip;

    [SerializeField] float torsoTiltScale = 1;
    [SerializeField] float shoulderTiltScale = 1;

    [Header("Arm")]
    [SerializeField] Vector3 armSwingScale = new Vector3(1, 1, 1);
    [SerializeField] float locomotionCentrifugalScale;
    [SerializeField] float parkourCentrifugalScale;

    [Header("Hand")]
    [SerializeField] float handAngleScale = 1;

    [Range(-180, 0)]
    [SerializeField] float handAngleLowerLimit, handAngleUpperLimit;

    [SerializeField] Bounds handSnapBounds;
    [SerializeField] LayerMask handSnapLayer;

    Vector3 velocity = Vector3.zero;
    Vector3 acceleration = Vector3.zero;
    float climbingTimer = 0;

    Stance stance;

    State state
    {
        get
        {
            if (abilityRunner.currentAbility is ClimbingAbility) return State.Climbing;
            else if (abilityRunner.currentAbility is ParkourAbility) return State.Parkouring;
            else if (velocity.magnitude > 0) return State.Running;
            else return State.Standing;
        }
    }

    void Start()
    {
        hip.nominal = robot.body.localPosition.y;
        hip.height = hip.stance;

        stance = new Stance(robot, rig, climber);
    }

    void Update()
    {
        UpdateVelocityAndAcceleration();

        if (state == State.Climbing &&
            (abilityRunner.currentAbility as ClimbingAbility).state == ClimbingAbility.State.Climbing)
        {
            UpdateClimbing();

            climbingTimer += Time.deltaTime;
        }
        else
        {
            UpdateRoot();
            UpdateHip();
            UpdateTorso();

            // Feet placement and arms swing
            float level = 0;
            float delta = 0;
            float chestAngle = 0;

            var snapLeft = SnapHand(Side.Left, rig.handLeft);
            var snapRight = SnapHand(Side.Right, rig.handRight);

            UpdateLimb(Side.Left, robot.footLeft.position, level, rig.footLeft, rig.handRight, rig.pivotRight, snapRight, out delta);
            chestAngle -= delta;

            UpdateLimb(Side.Right, robot.footRight.position, level, rig.footRight, rig.handLeft, rig.pivotLeft, snapLeft, out delta);
            chestAngle += delta;

            // Shoulder tilt.
            chestAngle *= shoulderTiltScale;
            rig.chest.localRotation = Quaternion.Euler(0, chestAngle, 0) * stance.chestRotation;

            if (state == State.Parkouring || state == State.Climbing)
            {
                var rotation = Quaternion.FromToRotation(rig.shoulder.up, Vector3.up) * stance.chestRotation;
                rig.chest.localRotation = rig.chest.localRotation.Fallout(rotation, 10);

                UpdateSnapHand(Side.Left, rig.handLeft, snapLeft);
                UpdateSnapHand(Side.Right, rig.handRight, snapRight);
            }

            climbingTimer = 0;
        }
    }

    void OnDrawGizmos()
    {
        if (rig.pivotLeft && rig.pivotRight)
        {
            Gizmos.DrawRay(rig.pivotLeft.position, rig.pivotLeft.forward);
            Gizmos.DrawRay(rig.pivotRight.position, rig.pivotRight.forward);
        }

        Gizmos.DrawSphere(rig.handLeft.position, 0.1f);
        Gizmos.DrawSphere(rig.handRight.position, 0.1f);
        Gizmos.DrawSphere(rig.footLeft.position, 0.1f);
        Gizmos.DrawSphere(rig.footRight.position, 0.1f);

        Gizmos.matrix = robot.root.localToWorldMatrix * rig.chest.localToWorldMatrix;

        var center = handSnapBounds.center;
        var size = handSnapBounds.size;
        Gizmos.DrawWireCube(center, size);

        center.x = -center.x;
        Gizmos.DrawWireCube(center, size);
    }

    void UpdateVelocityAndAcceleration()
    {
        ref var synthesizer = ref abilityRunner.Synthesizer.Ref;
        Vector3 velocity = robot.root.TransformVector(synthesizer.CurrentVelocity);

        var acceleration = (velocity - this.velocity) / Time.deltaTime;
        this.velocity = velocity;
        this.acceleration = this.acceleration.Fallout(acceleration, 5);

        Debug.DrawRay(robot.root.position, this.acceleration, Color.red);
        Debug.DrawRay(robot.root.position, this.velocity, Color.blue);
    }

    void UpdateRoot()
    {
        rig.root.position = robot.root.position;
        rig.root.rotation = robot.root.rotation * stance.rootRotation;
    }

    void UpdateHip()
    {
        float target = 0;
        if (state == State.Standing) target = hip.stance;
        else if (state == State.Running) target = hip.locomotion;
        else if (state == State.Parkouring || state == State.Climbing) target = hip.parkour;

        hip.height = hip.height.Fallout(target, 10);

        var position = robot.body.localPosition;
        position.y += hip.height - hip.nominal;
        rig.hip.localPosition = position;
        rig.hip.localRotation = robot.body.localRotation * stance.hipRotation;
    }

    void UpdateTorso()
    {
        var acceleration = robot.root.InverseTransformDirection(this.acceleration);
        var velocity = robot.root.InverseTransformDirection(this.velocity);
        Quaternion rotation;

        if (state == State.Parkouring || state == State.Climbing)
        {
            var up = rig.torso.up;
            // rotation = Quaternion.FromToRotation(up, Vector3.up) * torsoRotation;
            // rotation = Quaternion.Slerp(rotation, torsoRotation, 0.5f);
            rotation = Quaternion.Euler(torsoTiltScale * Vector3.Cross(Vector3.up, acceleration)) * stance.torsoRotation;
        }
        else
        {
            // var tilt = new Vector3(acceleration.x, 0, velocity.z);
            var tilt = acceleration + velocity;
            rotation = Quaternion.Euler(torsoTiltScale * Vector3.Cross(Vector3.up, tilt)) * stance.torsoRotation;
        }
        rig.torso.localRotation = rig.torso.localRotation.Fallout(rotation, 10);
    }

    void UpdateLimb(Side side, Vector3 footPosition, float level, Transform foot, Transform hand, Transform pivot, Snap snap, out float delta)
    {
        var position = robot.root.InverseTransformPoint(footPosition);
        foot.position = position;

        // For left foot, we are dealing with right hand, so take positive value.
        var sign = (int)side;

        position = rig.hip.InverseTransformPoint(position);
        delta = position.z - stance.nominal;
        var acceleration = robot.root.InverseTransformDirection(this.acceleration);
        var direction = armSwingScale.z * delta * pivot.forward
                        + armSwingScale.y * delta * delta * Vector3.up
                        - armSwingScale.x * delta * delta * sign * pivot.right;

        float centrifugalScale;
        if (state == State.Parkouring || state == State.Climbing) centrifugalScale = parkourCentrifugalScale * Mathf.Abs(acceleration.x);
        else centrifugalScale = locomotionCentrifugalScale * Mathf.Abs(acceleration.x);

        var centerfugal = centrifugalScale * sign * pivot.right;

        if (!snap.valid)
        {
            hand.position = hand.position.Fallout(pivot.position + direction, 10).Fallout(pivot.position + centerfugal, 1);

            var rotation = Quaternion.Euler(0, 0, sign * Mathf.Clamp(-180 + handAngleScale * centrifugalScale, handAngleLowerLimit, handAngleUpperLimit));
            hand.localRotation = hand.localRotation.Fallout(rotation, 10);
        }
    }

    bool SnapHand(Side side, Transform hand, out Vector3 point, out Vector3 direction)
    {
        RaycastHit hit;

        var origin = robot.root.position - 5 * robot.root.forward;
        var radius = 0.1f;
        if (Physics.SphereCast(origin, radius, robot.root.forward, out hit, 10, handSnapLayer) && hit.collider is BoxCollider)
        {
            // Get the upper platform.
            var collider = hit.collider as BoxCollider;
            var vertices = new Vector3[4];

            Vector3 center = collider.center;
            Vector3 size = collider.size;

            vertices[0] = collider.transform.TransformPoint(center + new Vector3(-size.x, size.y, size.z) * 0.5f);
            vertices[1] = collider.transform.TransformPoint(center + new Vector3(size.x, size.y, size.z) * 0.5f);
            vertices[2] = collider.transform.TransformPoint(center + new Vector3(size.x, size.y, -size.z) * 0.5f);
            vertices[3] = collider.transform.TransformPoint(center + new Vector3(-size.x, size.y, -size.z) * 0.5f);

            // Hand position in world space.
            AffineTransform contactTransform = new AffineTransform(center, quaternion.identity);
            float3 p = robot.root.TransformPoint(hand.position);
            float minimumDistance = float.MaxValue;

            for (int i = 0; i < 4; ++i)
            {
                int j = (i + 1) % 4;

                var candidateTransform = TagExtensions.GetClosestTransform(vertices[i], vertices[j], p);
                float distance = math.length(candidateTransform.t - p);
                if (distance < minimumDistance)
                {
                    minimumDistance = distance;
                    contactTransform = candidateTransform;
                }
            }

            point = contactTransform.t;
            direction = Missing.zaxis(contactTransform.q);

            center = handSnapBounds.center;
            center.x *= (int)side;
            Bounds bounds = new Bounds(center, handSnapBounds.size);

            var localPoint = rig.chest.InverseTransformPoint(robot.root.InverseTransformPoint(point));
            return bounds.Contains(localPoint);
        }
        else
        {
            point = direction = Vector3.zero;
            return false;
        }
    }

    Snap SnapHand(Side side, Transform hand)
    {
        Snap snap = new Snap();
        snap.valid = SnapHand(side, hand, out snap.point, out snap.direction);
        return snap;
    }

    void UpdateSnapHand(Side side, Transform hand, Snap snap)
    {
        if (!snap.valid) return;

        var position = robot.root.InverseTransformPoint(snap.point);
        var direction = robot.root.InverseTransformVector(snap.direction);

        Quaternion rotation;
        if (side == Side.Left) rotation = Quaternion.Euler(0, 90, 90);
        else rotation = Quaternion.Euler(0, -90, -90);

        hand.position = hand.position.Fallout(position, 10);
        hand.rotation = Quaternion.LookRotation(direction, Vector3.up) * rotation;

        Debug.DrawRay(snap.point, snap.direction, Color.blue);
    }

    void UpdateClimbing()
    {
        float damp = climbingTimer < 1 ? 10 : float.MaxValue;

        rig.root.position = climber.root.position;
        rig.root.rotation = climber.root.rotation * Quaternion.Inverse(stance.climberRotation) * stance.rootRotation;

        AffineTransform root;
        root.t = climber.root.position;
        root.q = climber.root.rotation * Quaternion.Inverse(stance.climberRotation);

        rig.hip.localPosition = stance.climberRotation * climber.body.localPosition;
        rig.hip.localRotation = climber.body.localRotation;

        rig.torso.localRotation = stance.torsoRotation;

        rig.handLeft.position = rig.handLeft.position.Fallout(root.inverseTransform(climber.handLeft.position), damp);
        rig.handRight.position = rig.handRight.position.Fallout(root.inverseTransform(climber.handRight.position), damp);
        rig.footLeft.position = rig.footLeft.position.Fallout(root.inverseTransform(climber.footLeft.position), damp);
        rig.footRight.position = rig.footRight.position.Fallout(root.inverseTransform(climber.footRight.position), damp);

        rig.handLeft.rotation = Quaternion.Euler(0, 90, 30);
        rig.handRight.rotation = Quaternion.Euler(0, -90, -30);
    }
}