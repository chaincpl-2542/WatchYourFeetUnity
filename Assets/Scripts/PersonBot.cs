using UnityEngine;

public class PersonBot : MonoBehaviour
{
    [Header("Main Bones")]
    public Transform head;

    [Header("Left Side")]
    public Transform leftShoulder;
    public Transform leftUpperArm;
    public Transform leftLowerArm;

    [Header("Right Side")]
    public Transform rightShoulder;
    public Transform rightUpperArm;
    public Transform rightLowerArm;

    [Header("Left Leg")]
    public Transform leftUpperLeg;
    public Transform leftLowerLeg;
    public Transform leftFoot;

    [Header("Right Leg")]
    public Transform rightUpperLeg;
    public Transform rightLowerLeg;
    public Transform rightFoot;
}
