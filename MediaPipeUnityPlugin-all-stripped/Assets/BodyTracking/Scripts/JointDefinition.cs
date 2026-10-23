using System;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
  [Serializable]
  public class JointDefinition
  {
    public string jointName;   // Ej: "Right elbow", "Left knee"
    public BodyGroup group;

    [Header("Joints (vertex in the middle)")]
    [Range(0, 32), Tooltip("Upper point (e.g. shoulder)")] public int jointA;
    [Range(0, 32), Tooltip("Middle point / vertex (e.g. elbow)")] public int jointB;
    [Range(0, 32), Tooltip("Lower point (e.g. wrist)")] public int jointC;

    [HideInInspector] public float currentAngle;

    public JointDefinition() { }

    public JointDefinition(string name, BodyGroup group, int a, int b, int c)
    {
      jointName = name;
      this.group = group;
      jointA = a;
      jointB = b;
      jointC = c;
    }

    // Articulaciones que vienen configuradas por defecto
    public static JointDefinition[] Defaults() => new[]
    {
      new JointDefinition("Right shoulder", BodyGroup.RightArm, RightHip, RightShoulder, RightElbow),
      new JointDefinition("Right elbow", BodyGroup.RightArm, RightShoulder, RightElbow, RightWrist),
      new JointDefinition("Left shoulder", BodyGroup.LeftArm, LeftHip, LeftShoulder, LeftElbow),
      new JointDefinition("Left elbow", BodyGroup.LeftArm, LeftShoulder, LeftElbow, LeftWrist),
      new JointDefinition("Right hip", BodyGroup.RightLeg, RightShoulder, RightHip, RightKnee),
      new JointDefinition("Right knee", BodyGroup.RightLeg, RightHip, RightKnee, RightAnkle),
      new JointDefinition("Right ankle", BodyGroup.RightLeg, RightKnee, RightAnkle, RightFootIndex),
      new JointDefinition("Left hip", BodyGroup.LeftLeg, LeftShoulder, LeftHip, LeftKnee),
      new JointDefinition("Left knee", BodyGroup.LeftLeg, LeftHip, LeftKnee, LeftAnkle),
      new JointDefinition("Left ankle", BodyGroup.LeftLeg, LeftKnee, LeftAnkle, LeftFootIndex),
    };
  }
}
