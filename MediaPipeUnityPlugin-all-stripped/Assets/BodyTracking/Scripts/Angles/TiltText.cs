using UnityEngine;

namespace BodyTracking
{
  // Convierte un ángulo con signo en texto: "45º right", "45º left" o "0º"
  public static class TiltText
  {
    public static string Format(float signedAngle, bool invertSides = false, float deadZone = 1f)
    {
      float abs = Mathf.Abs(signedAngle);
      if (abs < deadZone) return "0º";

      // Ángulo negativo = inclinada hacia la izquierda de la persona
      bool towardLeft = signedAngle < 0f;
      if (invertSides) towardLeft = !towardLeft;

      return $"{abs:F0}º {(towardLeft ? "left" : "right")}";
    }
  }
}
