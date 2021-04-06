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

    enum Side { Left, Right }

    [SerializeField] Rig rig;
    [SerializeField] Robot robot;
    [SerializeField] Transform root;

    [SerializeField] float stanceHipHeight = 0.65f;
    [SerializeField] float runningHipHeight = 0.55f;

    [SerializeField] float torsoTiltScale = 1;
    [SerializeField] float shoulderTiltScale = 1;

    [SerializeField] float footHeightOffset = 0;
    [SerializeField] float footHeightScale = 1;

    [SerializeField] Vector3 armScale = new Vector3(1, 1, 1);
    [SerializeField] float handAngleScale = 1;

    float hipHeight;
    float nominalHeight;
    float nominalStance;

    Vector3 velocity = Vector3.zero;
    Vector3 acceleration = Vector3.zero;

    Quaternion torsoRotation;
    Quaternion chestRotation;

    Vector3 rootPosition;
    float rootAngle;

    void Start()
    {
        nominalHeight = robot.body.position.y;
        nominalStance = (robot.footLeft.localPosition.z + robot.footRight.localPosition.z) / 2;

        hipHeight = stanceHipHeight;
        torsoRotation = rig.torso.localRotation;
        chestRotation = rig.chest.localRotation;

        rootPosition = root.position;
        rootAngle = root.eulerAngles.y * Mathf.Deg2Rad;
    }

    void Update()
    {
        // Hip height
        var velocity = robot.animator.velocity;
        if (velocity.magnitude == 0) hipHeight = hipHeight.Fallout(stanceHipHeight, 10);
        else hipHeight = hipHeight.Fallout(runningHipHeight, 10);

        var position = robot.body.localPosition;
        position.y += hipHeight - nominalHeight;
        rig.hip.localPosition = position;

        // Torso tilt
        var acceleration = (velocity - this.velocity) / Time.deltaTime;
        this.velocity = velocity;
        this.acceleration = this.acceleration.Fallout(acceleration, 5).Fallout(Vector3.zero, 1);
        Debug.DrawRay(root.position, this.acceleration, Color.red);

        acceleration = root.InverseTransformDirection(this.acceleration);
        var rotation = Quaternion.Euler(torsoTiltScale * Vector3.Cross(Vector3.up, acceleration)) * torsoRotation;
        rig.torso.localRotation = rig.torso.localRotation.Fallout(rotation, 10);

        // Feet placement and arms swing
        float level = 0;
        float delta = 0;
        float chestAngle = 0;

        MoveLimb(Side.Left, robot.footLeft.position, level, rig.footLeft, rig.handRight, rig.pivotRight, out delta);
        chestAngle -= delta;

        MoveLimb(Side.Right, robot.footRight.position, level, rig.footRight, rig.handLeft, rig.pivotLeft, out delta);
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

    void MoveLimb(Side side, Vector3 footPosition, float level, Transform foot, Transform hand, Transform pivot, out float delta)
    {
        var position = root.InverseTransformPoint(footPosition);
        position.y = footHeightScale * (position.y - level) + footHeightOffset;
        foot.position = position;

        // For left foot, we are dealing with right hand, so take positive value.
        var sign = side == Side.Left ? 1 : -1;

        delta = position.z - nominalStance;
        var acceleration = root.InverseTransformDirection(this.acceleration);
        var direction = armScale.z * delta * pivot.forward + armScale.y * delta * delta * Vector3.up;
        var centerfugal = armScale.x * Mathf.Abs(acceleration.x) * sign * pivot.right;
        hand.position = hand.position.Fallout(pivot.position + direction, 10).Fallout(pivot.position + centerfugal, 1);

        var rotation = Quaternion.Euler(0, 0, sign * (-180 + handAngleScale * Mathf.Abs(acceleration.x)));
        hand.localRotation = hand.localRotation.Fallout(rotation, 10);
    }
}