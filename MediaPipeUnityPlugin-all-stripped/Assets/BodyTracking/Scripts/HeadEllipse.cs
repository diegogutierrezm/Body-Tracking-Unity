using System;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
  [Serializable]
  public class HeadEllipseSettings
  {
    [Tooltip("Alto: veces la distancia ojos-boca")]
    public float heightFactor = 1.8f;
    [Tooltip("Ancho: veces la media distancia entre orejas")]
    public float widthFactor = 1.1f;
    [Range(0f, 1f), Tooltip("Lo más estrecho que puede quedar respecto al alto")]
    public float minWidthRatio = 0.6f;
    [Tooltip(">1 inclina más, <1 inclina menos")]
    public float rollFactor = 1.5f;
  }

  // Elipse de la cabeza: solo cálculo, no dibuja nada
  public readonly struct HeadEllipse
  {
    public readonly Vector2 Center;
    public readonly float RadiusX, RadiusY;
    public readonly Vector2 Right, Up; // ejes de la elipse ya inclinados

    private HeadEllipse(Vector2 center, float radiusX, float radiusY, float roll)
    {
      Center = center;
      RadiusX = radiusX;
      RadiusY = radiusY;
      Right = new Vector2(Mathf.Cos(roll), Mathf.Sin(roll));
      Up = new Vector2(-Right.y, Right.x);
    }

    // Punto más alto de la cabeza
    public Vector2 Top => Center + Up * RadiusY;

    // Punto del borde a partir del coseno y el seno de un ángulo
    public Vector2 PointAt(float cos, float sin) => Center + Right * (cos * RadiusX) + Up * (sin * RadiusY);

    public static HeadEllipse FromLandmarks(Vector2[] p, HeadEllipseSettings s)
    {
      var ears = (p[LeftEar] + p[RightEar]) * 0.5f;
      var eyes = (p[LeftEye] + p[RightEye]) * 0.5f;
      var mouth = (p[MouthLeft] + p[MouthRight]) * 0.5f;

      // Alto: no cambia al girar la cabeza. Ancho: se estrecha al girar.
      float radiusY = Vector2.Distance(eyes, mouth) * s.heightFactor;
      float radiusX = Vector2.Distance(p[LeftEar], p[RightEar]) * 0.5f * s.widthFactor;
      radiusX = Mathf.Clamp(radiusX, radiusY * s.minWidthRatio, radiusY);

      // Inclinación: media de ojos, orejas y eje vertical de la cara
      float roll = (LineAngle(p[LeftEye] - p[RightEye])
                  + LineAngle(p[LeftEar] - p[RightEar])
                  + LineAngle(eyes - mouth, -Mathf.PI / 2f)) / 3f * s.rollFactor;

      return new HeadEllipse(ears, radiusX, radiusY, roll);
    }

    // Ángulo de una línea respecto a la horizontal, entre -90° y 90° (en radianes)
    private static float LineAngle(Vector2 v, float offset = 0f)
    {
      float a = Mathf.Atan2(v.y, v.x) + offset;
      if (a > Mathf.PI / 2f) a -= Mathf.PI;
      else if (a < -Mathf.PI / 2f) a += Mathf.PI;
      return a;
    }
  }
}
