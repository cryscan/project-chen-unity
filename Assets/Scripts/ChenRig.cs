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
        public Transform handDirectionLeft, handDirectionRight;
    }

    [System.Serializable]
    public struct Robot
    {
        public Animator animator;
        public Transform body;
        public Transform footLeft, footRight;
    }

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
        Vector3 direction;
        Vector3 centerfugal;

        // Simulate arm inertia.
        var deltaRootPosition = root.position - rootPosition;
        var deltaRootAngle = root.eulerAngles.y * Mathf.Deg2Rad - rootAngle;

        rootPosition = root.position;
        rootAngle = root.eulerAngles.y * Mathf.Deg2Rad;

        position = root.InverseTransformPoint(robot.footLeft.position);
        position.y = footHeightScale * (position.y - level) + footHeightOffset;
        rig.footLeft.position = position;

        delta = position.z - nominalStance;
        chestAngle = -delta;
        direction = armScale.z * delta * rig.handDirectionRight.forward + armScale.y * delta * delta * rig.handDirectionRight.up;
        centerfugal = armScale.x * Mathf.Abs(acceleration.x) * (rig.handDirectionRight.right + rig.handDirectionRight.up / 2);
        position = rig.handDirectionRight.position;
        rig.handRight.position = rig.handRight.position.Fallout(position + direction, 10).Fallout(position + centerfugal, 1);

        rotation = Quaternion.Euler(0, 0, -180 + handAngleScale * Mathf.Abs(acceleration.x));
        rig.handRight.localRotation = rig.handRight.localRotation.Fallout(rotation, 10);

        position = root.InverseTransformPoint(robot.footRight.position);
        position.y = footHeightScale * (position.y - level) + footHeightOffset;
        rig.footRight.position = position;

        delta = position.z - nominalStance;
        chestAngle += delta;
        direction = armScale.z * delta * rig.handDirectionLeft.forward + armScale.y * delta * delta * rig.handDirectionLeft.up;
        centerfugal = armScale.x * Mathf.Abs(acceleration.x) * (-rig.handDirectionLeft.right + rig.handDirectionLeft.up / 2);
        position = rig.handDirectionLeft.position;
        rig.handLeft.position = rig.handLeft.position.Fallout(position + direction, 10).Fallout(position + centerfugal, 1);

        rotation = Quaternion.Euler(0, 0, 180 - handAngleScale * Mathf.Abs(acceleration.x));
        rig.handLeft.localRotation = rig.handLeft.localRotation.Fallout(rotation, 10);

        // Shoulder tilt.
        chestAngle *= shoulderTiltScale;
        rig.chest.localRotation = Quaternion.Euler(0, chestAngle, 0) * chestRotation;
    }

    void OnDrawGizmos()
    {
        if (rig.handDirectionLeft && rig.handDirectionRight)
        {
            Gizmos.DrawRay(rig.handDirectionLeft.position, rig.handDirectionLeft.forward);
            Gizmos.DrawRay(rig.handDirectionRight.position, rig.handDirectionRight.forward);
        }
    }
}