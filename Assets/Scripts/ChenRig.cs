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

        var delta = hipHeight - nominalHeight;
        var position = robot.body.localPosition;
        position.y += delta;
        var level = position.y;
        rig.hip.localPosition = position;

        var translation = new Vector3(-robot.body.position.x, 0, -robot.body.position.z);
        var rotation = Quaternion.Euler(0, -robot.body.rotation.eulerAngles.y, 0);

        position = rotation * (robot.footLeft.position + translation);
        rig.footLeft.position = position;

        position = rotation * (robot.footRight.position + translation);
        rig.footRight.position = position;

        var acceleration = (velocity - this.velocity) / Time.deltaTime;
        this.velocity = velocity;
        this.acceleration = this.acceleration.Fallout(acceleration, 5).Fallout(Vector3.zero, 1);
        Debug.DrawRay(-translation, this.acceleration, Color.red);

        acceleration = rotation * this.acceleration;
        rotation = Quaternion.Euler(torsoTiltScale * new Vector3(acceleration.z, 0, -acceleration.x)) * torsoRotation;
        rig.torso.localRotation = rig.torso.localRotation.Fallout(rotation, 10);

        // position.y = -level + footHeightScale * (position.y + level) - delta;
        // rig.footLeft.localPosition = robot.footLeft.localPosition;
        // position.y = -level + footHeightScale * (position.y + level) - delta;
        // rig.footLeft.Translate(new Vector3(0, footHeightOffset, 0), Space.World);

        // rig.footRight.localPosition = robot.footRight.localPosition;
        // rig.footRight.Translate(new Vector3(0, footHeightOffset, 0), Space.World);
        // position.y = -level + footHeightScale * (position.y + level) - delta;
        // rig.footRight.position = position;
    }
}