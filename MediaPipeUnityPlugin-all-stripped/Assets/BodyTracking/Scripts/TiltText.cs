using UnityEngine;

namespace BodyTracking
{
  // Convierte un ángulo con signo en texto: "45º derecha", "45º izq" o "0º"
  public static class TiltText
  {
    public static string Format(float signedAngle, bool invertSides = false, float deadZone = 1f)
    {
      float abs = Mathf.Abs(signedAngle);
      if (abs < deadZone) return "0º";

      bool right = signedAngle < 0f;
      if (invertSides) right = !right;

      return $"{abs:F0}º {(right ? "izq" : "der")}";
    }
  }
}
