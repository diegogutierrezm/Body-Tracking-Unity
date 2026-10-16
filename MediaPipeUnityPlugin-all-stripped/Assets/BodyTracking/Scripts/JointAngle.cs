using UnityEngine;

namespace BodyTracking
{
  public static class JointAngle
  {
    // Ángulo en B formado por A-B-C, de 0 a 180 grados
    public static float Compute(Vector2 a, Vector2 b, Vector2 c) => Vector2.Angle(a - b, c - b);

    // Igual, pero en 3D (tiene en cuenta la profundidad)
    public static float Compute(Vector3 a, Vector3 b, Vector3 c) => Vector3.Angle(a - b, c - b);
  }
}
