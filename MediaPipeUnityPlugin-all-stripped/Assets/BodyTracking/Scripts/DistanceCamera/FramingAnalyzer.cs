using System;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
    [Serializable]
    public class FramingSettings
    {
        [Range(0f, 0.2f), Tooltip("Minimum margin to the image edge (0.03 = 3 %)")]
        public float edgeMargin = 0.03f;
        [Range(0f, 1f), Tooltip("Minimum body height relative to the image. Below it: move closer")]
        public float minBodyHeight = 0.6f;
        [Tooltip("Head top = eyes + X times the eyes-mouth distance")]
        public float headTopFactor = 1.7f;
    }

    // Qué partes del cuerpo se salen de la imagen y cuánto ocupa
    public readonly struct FramingReport
    {
        public readonly bool CutTop, CutBottom, CutLeft, CutRight;
        public readonly float BodyHeight; // fracción de la altura de la imagen

        public FramingReport(bool cutTop, bool cutBottom, bool cutLeft, bool cutRight, float bodyHeight)
        {
            CutTop = cutTop;
            CutBottom = cutBottom;
            CutLeft = cutLeft;
            CutRight = cutRight;
            BodyHeight = bodyHeight;
        }

        public bool FullBodyVisible => !CutTop && !CutBottom && !CutLeft && !CutRight;
    }

    public static class FramingAnalyzer
    {
        // p: posiciones de los 33 puntos en 0-1 (origen abajo a la izquierda)
        public static FramingReport Analyze(Vector2[] p, FramingSettings s)
        {
            // Coronilla: por encima de los ojos, proporcional a la distancia ojos-boca
            var eyes = (p[LeftEye] + p[RightEye]) * 0.5f;
            var mouth = (p[MouthLeft] + p[MouthRight]) * 0.5f;
            float headTop = eyes.y + (eyes.y - mouth.y) * s.headTopFactor;

            // Pies: el punto más bajo entre tobillos, talones y puntas (27-32)
            float feet = float.MaxValue;
            for (int i = LeftAnkle; i < Count; i++) feet = Mathf.Min(feet, p[i].y);

            // Lados: el punto más a la izquierda y el más a la derecha
            float minX = float.MaxValue, maxX = float.MinValue;
            for (int i = 0; i < Count; i++)
            {
                minX = Mathf.Min(minX, p[i].x);
                maxX = Mathf.Max(maxX, p[i].x);
            }

            float m = s.edgeMargin;
            return new FramingReport(
              cutTop: headTop > 1f - m,
              cutBottom: feet < m,
              cutLeft: minX < m,
              cutRight: maxX > 1f - m,
              bodyHeight: headTop - feet);
        }
    }
}