using System;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
  [Serializable]
  public class JointDefinition
  {
    public string jointName;   // Ej: "Codo der", "Rodilla izq"
    public BodyGroup group;

    [Header("Articulaciones (origen en el medio)")]
    [Range(0, 32), Tooltip("Punto superior (ej: hombro)")] public int jointA;
    [Range(0, 32), Tooltip("Punto medio/pivote (ej: codo)")] public int jointB;
    [Range(0, 32), Tooltip("Punto inferior (ej: muñeca)")] public int jointC;

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
      new JointDefinition("Hombro der", BodyGroup.RightArm, RightHip, RightShoulder, RightElbow),
      new JointDefinition("Codo der", BodyGroup.RightArm, RightShoulder, RightElbow, RightWrist),
      new JointDefinition("Hombro izq", BodyGroup.LeftArm, LeftHip, LeftShoulder, LeftElbow),
      new JointDefinition("Codo izq", BodyGroup.LeftArm, LeftShoulder, LeftElbow, LeftWrist),
      new JointDefinition("Cadera der", BodyGroup.RightLeg, RightShoulder, RightHip, RightKnee),
      new JointDefinition("Rodilla der", BodyGroup.RightLeg, RightHip, RightKnee, RightAnkle),
      new JointDefinition("Tobillo der", BodyGroup.RightLeg, RightKnee, RightAnkle, RightFootIndex),
      new JointDefinition("Cadera izq", BodyGroup.LeftLeg, LeftShoulder, LeftHip, LeftKnee),
      new JointDefinition("Rodilla izq", BodyGroup.LeftLeg, LeftHip, LeftKnee, LeftAnkle),
      new JointDefinition("Tobillo izq", BodyGroup.LeftLeg, LeftKnee, LeftAnkle, LeftFootIndex),
    };
  }
}
