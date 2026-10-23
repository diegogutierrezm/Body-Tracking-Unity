using System;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
    [Serializable]
    public class HeadEllipseSettings
    {
        [Tooltip("Height: times the eyes-mouth distance")]
        public float heightFactor = 1.8f;
        [Tooltip("Width: times half the distance between the ears")]
        public float widthFactor = 1.1f;
        [Range(0f, 1f), Tooltip("Narrowest width allowed, relative to the height")]
        public float minWidthRatio = 0.6f;
        [Tooltip(">1 tilts more, <1 tilts less")]
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

            // Inclinación: media de ojos, orejas y eje de la cara, ponderada por la longitud de cada línea.
            // De perfil, ojos y orejas quedan casi juntos: esas líneas son cortas y casi no cuentan.
            var faceAxis = eyes - mouth;                                // apunta hacia arriba
            var faceAxisAsHorizontal = new Vector2(faceAxis.y, -faceAxis.x); // girado 90°, para tratarlo como horizontal
            float minLength = radiusY * 0.5f;                           // más cortas que esto: su ángulo es ruido

            var sum = Vector2.zero;
            AddLine(ref sum, p[LeftEye] - p[RightEye], minLength);
            AddLine(ref sum, p[LeftEar] - p[RightEar], minLength);
            AddLine(ref sum, faceAxisAsHorizontal, 0f);                  // el eje de la cara siempre cuenta

            float roll = sum.sqrMagnitude < 0.000001f ? 0f : Mathf.Atan2(sum.y, sum.x) * 0.5f * s.rollFactor;

            return new HeadEllipse(ears, radiusX, radiusY, roll);
        }

        // Ángulo de una línea respecto a la horizontal, entre -90° y 90° (en radianes)
        // Suma una línea a la media de inclinación, con un peso igual a su longitud.
        // Usa el "ángulo doble": así una línea a 0° y la misma a 180° cuentan igual, y no hay saltos de ±90°.
        private static void AddLine(ref Vector2 sum, Vector2 line, float minLength)
        {
            float length = line.magnitude;
            if (length < minLength || length < 0.0001f) return; // demasiado corta: no cuenta

            float doubleAngle = Mathf.Atan2(line.y, line.x) * 2f;
            sum += new Vector2(Mathf.Cos(doubleAngle), Mathf.Sin(doubleAngle)) * length;
        }
    }
}
