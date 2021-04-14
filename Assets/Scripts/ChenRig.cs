using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Kinematica;
using Unity.Mathematics;

public class ChenRig : MonoBehaviour
{
    enum State { Standing, Running, Parkouring, Climbing }
    enum Side { Left, Right }

    [System.Serializable]
    public struct Rig
    {
        public Transform hip;
        public Transform torso;
        public Transform chest;
        public Transform footLeft, footRight;
        public Transform handLeft, handRight;
        public Transform pivotLeft, pivotRight;
    }

    [System.Serializable]
    public struct Robot
    {
        public Transform body;
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

    struct Snap
    {
        public bool valid;
        public Vector3 point;
        public Vector3 direction;
    }

    [SerializeField] AbilityRunner abilityRunner;
    [SerializeField] Rig rig;
    [SerializeField] Robot robot;
    [SerializeField] Transform root;
    [SerializeField] Hip hip;

    [SerializeField] float torsoTiltScale = 1;
    [SerializeField] float shoulderTiltScale = 1;

    [Header("Arm")]
    [SerializeField] Vector3 armSwingScale = new Vector3(1, 1, 1);
    [SerializeField] float locomotionCentrifugalScale;
    [SerializeField] float wallCentrifugalScale;
    [SerializeField] float platformCentrifugalScale;

    [Header("Hand")]
    [SerializeField] float handAngleScale = 1;

    [Range(-180, 0)]
    [SerializeField] float handAngleLowerLimit, handAngleUpperLimit;

    [SerializeField] LayerMask snapLayer;
    [SerializeField] float snapDetectionHeight = 0.6f;
    [SerializeField] float snapDetectionRange = 1;

    float nominalStance;

    Vector3 velocity = Vector3.zero;
    Vector3 acceleration = Vector3.zero;

    Quaternion hipRotation;
    Quaternion torsoRotation;
    Quaternion chestRotation;

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
        hip.nominal = robot.body.position.y;
        nominalStance = (robot.footLeft.localPosition.z + robot.footRight.localPosition.z) / 2;

        hip.height = hip.stance;

        hipRotation = rig.hip.localRotation;
        torsoRotation = rig.torso.localRotation;
        chestRotation = rig.chest.localRotation;
    }

    void Update()
    {
        UpdateVelocityAndAcceleration();

        UpdateHip();
        UpdateTorso();

        // Feet placement and arms swing
        float level = 0;
        float delta = 0;
        float chestAngle = 0;

        var snapLeft = SnapHand(rig.handLeft);
        var snapRight = SnapHand(rig.handRight);

        UpdateLimb(Side.Left, robot.footLeft.position, level, rig.footLeft, rig.handRight, rig.pivotRight, snapRight, out delta);
        chestAngle -= delta;

        UpdateLimb(Side.Right, robot.footRight.position, level, rig.footRight, rig.handLeft, rig.pivotLeft, snapLeft, out delta);
        chestAngle += delta;

        // Shoulder tilt.
        chestAngle *= shoulderTiltScale;
        rig.chest.localRotation = Quaternion.Euler(0, chestAngle, 0) * chestRotation;

        if (state == State.Parkouring)
        {
            UpdateSnapHand(Side.Left, rig.handLeft, snapLeft);
            UpdateSnapHand(Side.Right, rig.handRight, snapRight);
        }
    }

    void OnDrawGizmos()
    {
        if (rig.pivotLeft && rig.pivotRight)
        {
            Gizmos.DrawRay(rig.pivotLeft.position, rig.pivotLeft.forward);
            Gizmos.DrawRay(rig.pivotRight.position, rig.pivotRight.forward);
        }
    }

    void UpdateVelocityAndAcceleration()
    {
        ref var synthesizer = ref abilityRunner.Synthesizer.Ref;
        Vector3 velocity = synthesizer.CurrentVelocity;

        var acceleration = (velocity - this.velocity) / Time.deltaTime;
        this.velocity = velocity;
        this.acceleration = this.acceleration.Fallout(acceleration, 5);
        Debug.DrawRay(root.position, this.acceleration, Color.red);
    }

    void UpdateHip()
    {
        float target = 0;
        if (state == State.Standing) target = hip.stance;
        else if (state == State.Running) target = hip.locomotion;
        else if (state == State.Parkouring) target = hip.parkour;

        hip.height = hip.height.Fallout(target, 10);

        var position = robot.body.localPosition;
        position.y += hip.height - hip.nominal;
        rig.hip.localPosition = position;
        rig.hip.localRotation = robot.body.localRotation * hipRotation;
    }

    void UpdateTorso()
    {
        var acceleration = root.InverseTransformDirection(this.acceleration);
        Quaternion rotation;
        if (state == State.Parkouring)
        {
            var up = rig.torso.rotation * Vector3.up;
            rotation = Quaternion.FromToRotation(up, Vector3.up) * torsoRotation;
            rotation = Quaternion.Slerp(rotation, torsoRotation, 0.5f);
            rotation = Quaternion.Euler(torsoTiltScale * Vector3.Cross(Vector3.up, acceleration)) * rotation;
        }
        else
        {
            float factor = 0.3f;
            var velocityTilt = Vector3.Cross(Vector3.up, velocity);
            var accelerationTilt = Vector3.Cross(Vector3.up, acceleration);
            rotation = Quaternion.Euler(torsoTiltScale * (velocityTilt * factor + accelerationTilt * (1 - factor))) * torsoRotation;
        }
        rig.torso.localRotation = rig.torso.localRotation.Fallout(rotation, 10);
    }

    void UpdateLimb(Side side, Vector3 footPosition, float level, Transform foot, Transform hand, Transform pivot, Snap snap, out float delta)
    {
        var position = root.InverseTransformPoint(footPosition);
        foot.position = position;

        // For left foot, we are dealing with right hand, so take positive value.
        var sign = side == Side.Left ? 1 : -1;

        delta = position.z - nominalStance;
        var acceleration = root.InverseTransformDirection(this.acceleration);
        var direction = armSwingScale.z * delta * pivot.forward
                        + armSwingScale.y * delta * delta * Vector3.up
                        - armSwingScale.x * delta * delta * sign * pivot.right;

        float centrifugalScale;
        if (state == State.Parkouring)
        {
            centrifugalScale = 0;
            var ability = abilityRunner.currentAbility as ParkourAbility;
            if (ability.parkour.IsType(Parkour.Type.Wall)) centrifugalScale = wallCentrifugalScale * Mathf.Abs(acceleration.x);
            else if (ability.parkour.IsType(Parkour.Type.Platform)) centrifugalScale = platformCentrifugalScale * acceleration.magnitude;
        }
        else centrifugalScale = locomotionCentrifugalScale * Mathf.Abs(acceleration.x);

        var centerfugal = centrifugalScale * sign * pivot.right;

        if (!snap.valid)
        {
            hand.position = hand.position.Fallout(pivot.position + direction, 10).Fallout(pivot.position + centerfugal, 1);

            var rotation = Quaternion.Euler(0, 0, sign * Mathf.Clamp(-180 + handAngleScale * centrifugalScale, handAngleLowerLimit, handAngleUpperLimit));
            hand.localRotation = hand.localRotation.Fallout(rotation, 10);
        }
    }

    bool SnapHand(Transform hand, out Vector3 point, out Vector3 direction)
    {
        var sphereCenter = root.TransformPoint(new Vector3(0, snapDetectionHeight, snapDetectionRange / 2));
        var colliders = Physics.OverlapSphere(sphereCenter, snapDetectionRange / 2, snapLayer);

        if (colliders.Length > 0 && colliders[0] is BoxCollider)
        {
            // Get the upper platform.
            var collider = colliders[0] as BoxCollider;
            var vertices = new Vector3[4];

            Vector3 center = collider.center;
            Vector3 size = collider.size;

            vertices[0] = collider.transform.TransformPoint(center + new Vector3(-size.x, size.y, size.z) * 0.5f);
            vertices[1] = collider.transform.TransformPoint(center + new Vector3(size.x, size.y, size.z) * 0.5f);
            vertices[2] = collider.transform.TransformPoint(center + new Vector3(size.x, size.y, -size.z) * 0.5f);
            vertices[3] = collider.transform.TransformPoint(center + new Vector3(-size.x, size.y, -size.z) * 0.5f);

            // Hand position in world space.
            AffineTransform contactTransform = new AffineTransform(center, quaternion.identity);
            float3 p = root.TransformPoint(hand.position);
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

            return true;
        }
        else
        {
            point = direction = Vector3.zero;
            return false;
        }
    }

    Snap SnapHand(Transform hand)
    {
        Snap snap = new Snap();
        snap.valid = SnapHand(hand, out snap.point, out snap.direction);
        return snap;
    }

    void UpdateSnapHand(Side side, Transform hand, Snap snap)
    {
        if (!snap.valid) return;

        var position = root.InverseTransformPoint(snap.point);
        var direction = root.InverseTransformVector(snap.direction);

        Quaternion rotation;
        if (side == Side.Left) rotation = Quaternion.Euler(0, 90, 90);
        else rotation = Quaternion.Euler(0, -90, -90);

        hand.position = hand.position.Fallout(position, 10);
        hand.rotation = Quaternion.LookRotation(direction, Vector3.up) * rotation;

        Debug.DrawRay(snap.point, snap.direction, Color.blue);
    }
}