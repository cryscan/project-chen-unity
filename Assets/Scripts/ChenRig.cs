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
        public Transform footLeft, footRight;
        public Transform handLeft, handRight;
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

    [SerializeField] float footHeightOffset = 0;
    [SerializeField] float footHeightScale = 1;
    [SerializeField] float stanceHipHeight = 0.65f;
    [SerializeField] float runningHipHeight = 0.55f;

    [SerializeField] float torsoTiltScale = 1;

    float hipHeight;
    float nominalHeight;

    Vector3 velocity = Vector3.zero;
    Vector3 acceleration = Vector3.zero;

    Quaternion torsoRotation;

    void Start()
    {
        nominalHeight = robot.body.position.y;
        hipHeight = stanceHipHeight;

        torsoRotation = rig.torso.localRotation;
    }

    void Update()
    {
        var velocity = robot.animator.velocity;
        if (velocity.magnitude == 0) hipHeight = hipHeight.Fallout(stanceHipHeight, 10);
        else hipHeight = hipHeight.Fallout(runningHipHeight, 10);

        var position = robot.body.localPosition;
        position.y += hipHeight - nominalHeight;
        rig.hip.localPosition = position;

        float level = 0;

        position = root.InverseTransformPoint(robot.footLeft.position);
        position.y = footHeightScale * (position.y - level) + footHeightOffset;
        rig.footLeft.position = position;

        position = root.InverseTransformPoint(robot.footRight.position);
        position.y = footHeightScale * (position.y - level) + footHeightOffset;
        rig.footRight.position = position;

        var acceleration = (velocity - this.velocity) / Time.deltaTime;
        this.velocity = velocity;
        this.acceleration = this.acceleration.Fallout(acceleration, 5).Fallout(Vector3.zero, 1);
        Debug.DrawRay(root.position, this.acceleration, Color.red);

        acceleration = root.InverseTransformDirection(this.acceleration);
        var rotation = Quaternion.Euler(torsoTiltScale * Vector3.Cross(Vector3.up, acceleration)) * torsoRotation;
        rig.torso.localRotation = rig.torso.localRotation.Fallout(rotation, 10);
    }
}