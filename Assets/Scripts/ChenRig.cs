using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChenRig : MonoBehaviour
{
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
        public Animator animator;
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

    enum Side { Left, Right }

    [SerializeField] AbilityRunner abilityRunner;
    [SerializeField] Rig rig;
    [SerializeField] Robot robot;
    [SerializeField] Transform root;
    [SerializeField] Hip hip;

    [SerializeField] float torsoTiltScale = 1;
    [SerializeField] float shoulderTiltScale = 1;

    [SerializeField] Vector3 armSwingScale = new Vector3(1, 1, 1);
    [SerializeField] float handAngleScale = 1;
    [SerializeField] float parkourCentrifugalScale;

    float nominalStance;

    Vector3 velocity = Vector3.zero;
    Vector3 acceleration = Vector3.zero;

    Quaternion hipRotation;
    Quaternion torsoRotation;
    Quaternion chestRotation;

    Vector3 rootPosition;
    float rootAngle;

    IAbility currentAbility => abilityRunner.currentAbility;

    void Start()
    {
        hip.nominal = robot.body.position.y;
        nominalStance = (robot.footLeft.localPosition.z + robot.footRight.localPosition.z) / 2;

        hip.height = hip.stance;

        hipRotation = rig.hip.localRotation;
        torsoRotation = rig.torso.localRotation;
        chestRotation = rig.chest.localRotation;

        rootPosition = root.position;
        rootAngle = root.eulerAngles.y * Mathf.Deg2Rad;
    }

    void Update()
    {
        // Hip height
        var velocity = robot.animator.velocity;
        UpdateHip(velocity);

        var position = robot.body.localPosition;
        position.y += hip.height - hip.nominal;
        rig.hip.localPosition = position;
        // rig.hip.localRotation = (currentAbility is ParkourAbility) ? robot.body.localRotation : Quaternion.identity;
        rig.hip.localRotation = robot.body.localRotation * hipRotation;

        // Torso tilt
        var acceleration = (velocity - this.velocity) / Time.deltaTime;
        this.velocity = velocity;
        this.acceleration = this.acceleration.Fallout(acceleration, 5);
        Debug.DrawRay(root.position, this.acceleration, Color.red);

        acceleration = root.InverseTransformDirection(this.acceleration);
        var rotation = Quaternion.Euler(torsoTiltScale * Vector3.Cross(Vector3.up, acceleration)) * torsoRotation;
        rig.torso.localRotation = rig.torso.localRotation.Fallout(rotation, 10);

        // Feet placement and arms swing
        float level = 0;
        float delta = 0;
        float chestAngle = 0;

        UpdateLimb(Side.Left, robot.footLeft.position, level, rig.footLeft, rig.handRight, rig.pivotRight, out delta);
        chestAngle -= delta;

        UpdateLimb(Side.Right, robot.footRight.position, level, rig.footRight, rig.handLeft, rig.pivotLeft, out delta);
        chestAngle += delta;

        // Shoulder tilt.
        chestAngle *= shoulderTiltScale;
        rig.chest.localRotation = Quaternion.Euler(0, chestAngle, 0) * chestRotation;
    }

    void OnDrawGizmos()
    {
        if (rig.pivotLeft && rig.pivotRight)
        {
            Gizmos.DrawRay(rig.pivotLeft.position, rig.pivotLeft.forward);
            Gizmos.DrawRay(rig.pivotRight.position, rig.pivotRight.forward);
        }
    }

    void UpdateHip(Vector3 velocity)
    {
        float target = 0;

        if (velocity.magnitude == 0) target = hip.stance;
        else target = hip.locomotion;

        if (currentAbility is ParkourAbility) target = hip.parkour;

        hip.height = hip.height.Fallout(target, 10);
    }

    void UpdateLimb(Side side, Vector3 footPosition, float level, Transform foot, Transform hand, Transform pivot, out float delta)
    {
        var position = root.InverseTransformPoint(footPosition);
        foot.position = position;

        // For left foot, we are dealing with right hand, so take positive value.
        var sign = side == Side.Left ? 1 : -1;

        delta = position.z - nominalStance;
        var acceleration = root.InverseTransformDirection(this.acceleration);
        var direction = armSwingScale.z * delta * pivot.forward + armSwingScale.y * delta * delta * Vector3.up;

        float centrifugalScale;
        if (currentAbility is ParkourAbility) centrifugalScale = parkourCentrifugalScale;
        else centrifugalScale = Mathf.Abs(acceleration.x);

        var centerfugal = armSwingScale.x * centrifugalScale * sign * pivot.right;

        hand.position = hand.position.Fallout(pivot.position + direction, 10).Fallout(pivot.position + centerfugal, 1);

        var rotation = Quaternion.Euler(0, 0, sign * Mathf.Clamp(-180 + handAngleScale * centrifugalScale, -180, 0));
        hand.localRotation = hand.localRotation.Fallout(rotation, 10);
    }
}