using System;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
    public enum SmoothingLevel { Off, Low, Medium, High }

    // Los valores de un nivel de suavizado
    [Serializable]
    public struct SmoothingPreset
    {
        [Tooltip("Más bajo = menos temblor (pero más retraso)")] public float minCutoff;
        [Tooltip("Más alto = menos retraso en movimientos rápidos (en píxeles)")] public float beta;
        [Range(0.05f, 1f), Tooltip("Multiplica el Min Cutoff en manos y pies (más bajo = más suave)")] public float extremityFactor;
        [Range(0.01f, 1f), Tooltip("Multiplica el Min Cutoff en puntos poco fiables")] public float lowVisibilityFactor;
    }

    // Suaviza los puntos del esqueleto ANTES de que se dibujen las líneas y de que los usen los demás scripts.
    // Manos y pies se suavizan más, y los puntos que el modelo no ve bien, muchísimo más.
    [DefaultExecutionOrder(-100)] // se suscribe antes que los demás, así se ejecuta el primero
    public class SkeletonSmoother : MonoBehaviour
    {
        [SerializeField] private SmoothingLevel _level = SmoothingLevel.Medium;

        [Header("Valores de cada nivel")]
        [SerializeField] private SmoothingPreset _low = new SmoothingPreset { minCutoff = 3f, beta = 0.1f, extremityFactor = 0.8f, lowVisibilityFactor = 0.5f };
        [SerializeField] private SmoothingPreset _medium = new SmoothingPreset { minCutoff = 2f, beta = 0.05f, extremityFactor = 0.7f, lowVisibilityFactor = 0.3f };
        [SerializeField] private SmoothingPreset _high = new SmoothingPreset { minCutoff = 1f, beta = 0.02f, extremityFactor = 0.4f, lowVisibilityFactor = 0.15f };

        [Header("Visibilidad")]
        [SerializeField, Range(0f, 1f), Tooltip("Por debajo de esta visibilidad, el punto es poco fiable")]
        private float _minVisibility = 0.5f;

        // Para cambiar el nivel desde la UI
        public SmoothingLevel Level
        {
            get => _level;
            set { _level = value; _smoother.Reset(); } // al cambiar de nivel se empieza de cero, sin saltos raros
        }

        private readonly Vector2[] _points = new Vector2[Count];
        private readonly float[] _cutoffs = new float[Count];
        private readonly float[] _visibility = new float[Count];
        private readonly float[] _visibilityBuffer = new float[Count];
        private readonly object _lock = new object();
        private readonly PointSmoother _smoother = new PointSmoother(Count);

        private SmoothingPreset CurrentPreset =>
          _level == SmoothingLevel.Low ? _low :
          _level == SmoothingLevel.High ? _high : _medium;

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

            if (_level == SmoothingLevel.Off) return; // sin suavizado: los puntos se quedan como los da el modelo

            if (!landmarks[LeftShoulder].isActive || !landmarks[RightShoulder].isActive)
            {
                _smoother.Reset(); // no hay persona: se empieza de cero la próxima vez
                return;
            }

            lock (_lock) Array.Copy(_visibilityBuffer, _visibility, Count);

            var preset = CurrentPreset;
            for (int i = 0; i < Count; i++)
            {
                _points[i] = landmarks[i].transform.localPosition;

                float cutoff = preset.minCutoff;
                if (IsExtremity(i)) cutoff *= preset.extremityFactor;
                if (_visibility[i] < _minVisibility) cutoff *= preset.lowVisibilityFactor;
                _cutoffs[i] = cutoff;
            }

            _smoother.Apply(_points, Time.unscaledTime, _cutoffs, preset.beta);

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