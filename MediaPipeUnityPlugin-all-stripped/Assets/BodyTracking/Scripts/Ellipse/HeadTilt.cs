using UnityEngine;

namespace BodyTracking
{
  public static class HeadTilt
  {
    // Grados: 0 = cabeza recta respecto a los hombros. El signo indica hacia qué lado.
    public static float Compute(Vector2 leftShoulder, Vector2 rightShoulder, Vector2 headTop)
    {
      var neck = (leftShoulder + rightShoulder) * 0.5f;
      var shoulders = leftShoulder - rightShoulder;
      var perpendicular = new Vector2(-shoulders.y, shoulders.x);
      if (perpendicular.y < 0f) perpendicular = -perpendicular; // que apunte hacia arriba

      return Vector2.SignedAngle(perpendicular, headTop - neck);
    }
  }
}
