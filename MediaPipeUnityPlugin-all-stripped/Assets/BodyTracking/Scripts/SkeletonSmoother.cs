using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
    // Suaviza los puntos del esqueleto ANTES de que se dibujen las líneas y de que los usen los demás scripts.
    // Manos y pies se suavizan más, y los puntos que el modelo no ve bien, muchísimo más.
    [DefaultExecutionOrder(-100)] // se suscribe antes que los demás, así se ejecuta el primero
    public class SkeletonSmoother : MonoBehaviour
    {
        [SerializeField, Tooltip("Más bajo = menos temblor (pero más retraso)")] private float _minCutoff = 1f;
        [SerializeField, Tooltip("Más alto = menos retraso en movimientos rápidos (en píxeles)")] private float _beta = 0.005f;
        [SerializeField, Range(0.05f, 1f), Tooltip("Multiplica el Min Cutoff en manos y pies (más bajo = más suave)")]
        private float _extremityFactor = 0.4f;
        [SerializeField, Range(0f, 1f), Tooltip("Por debajo de esta visibilidad, el punto es poco fiable")]
        private float _minVisibility = 0.5f;
        [SerializeField, Range(0.01f, 1f), Tooltip("Multiplica el Min Cutoff en puntos poco fiables (casi quietos)")]
        private float _lowVisibilityFactor = 0.1f;

        private readonly Vector2[] _points = new Vector2[Count];
        private readonly float[] _cutoffs = new float[Count];
        private readonly float[] _visibility = new float[Count];
        private readonly float[] _visibilityBuffer = new float[Count];
        private readonly object _lock = new object();
        private readonly PointSmoother _smoother = new PointSmoother(Count);

        private void OnEnable()
        {
            PoseLandmarkListAnnotation.Drawn += OnPoseDrawn;
            PoseLandmarkerRunner.WorldLandmarksReceived += OnWorldLandmarks;
        }

        private void OnDisable()
        {
            PoseLandmarkListAnnotation.Drawn -= OnPoseDrawn;
            PoseLandmarkerRunner.WorldLandmarksReceived -= OnWorldLandmarks;
        }

        // Llega desde el hilo de MediaPipe: solo guarda la visibilidad de cada punto
        private void OnWorldLandmarks(IReadOnlyList<Landmark> landmarks)
        {
            if (landmarks == null || landmarks.Count < Count) return;
            lock (_lock)
            {
                for (int i = 0; i < Count; i++) _visibilityBuffer[i] = landmarks[i].visibility ?? 1f;
            }
        }

        private void OnPoseDrawn(PoseLandmarkListAnnotation annotation)
        {
            var landmarks = annotation.Points;
            if (landmarks.count < Count) return;

            if (!landmarks[LeftShoulder].isActive || !landmarks[RightShoulder].isActive)
            {
                _smoother.Reset(); // no hay persona: se empieza de cero la próxima vez
                return;
            }

            lock (_lock) System.Array.Copy(_visibilityBuffer, _visibility, Count);

            for (int i = 0; i < Count; i++)
            {
                _points[i] = landmarks[i].transform.localPosition;

                float cutoff = _minCutoff;
                if (IsExtremity(i)) cutoff *= _extremityFactor;
                if (_visibility[i] < _minVisibility) cutoff *= _lowVisibilityFactor;
                _cutoffs[i] = cutoff;
            }

            _smoother.Apply(_points, Time.unscaledTime, _cutoffs, _beta);

            // Vuelve a colocar los puntos ya suavizados (las líneas se dibujan después con estas posiciones)
            for (int i = 0; i < Count; i++)
            {
                var t = landmarks[i].transform;
                var p = t.localPosition;
                t.localPosition = new Vector3(_points[i].x, _points[i].y, p.z);
            }
        }

        // Manos (15-22) y pies (27-32)
        private static bool IsExtremity(int i) => (i >= LeftWrist && i <= 22) || i >= LeftAnkle;
    }
}