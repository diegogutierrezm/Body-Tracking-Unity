using System.Collections.Generic;
using Mediapipe.Unity;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
    // Comprueba si el cuerpo entero está dentro de la imagen y da instrucciones
    public class FramingMonitor : MonoBehaviour
    {
        [SerializeField] private FramingSettings _settings = new FramingSettings();
        [SerializeField, Tooltip("Segundos entre cada mensaje")] private float _logInterval = 1f;

        public FramingReport LastReport { get; private set; }
        public bool FullBodyVisible => LastReport.FullBodyVisible;

        // NUEVO: lo que hay que corregir. Si está vacía, el encuadre es correcto
        public IReadOnlyList<string> Messages => _messages;

        // NUEVO: true si se ha detectado a alguien hace menos de medio segundo
        public bool HasPerson => Time.time - _lastPoseTime < 0.5f;

        private readonly Vector2[] _points = new Vector2[Count];
        private readonly List<string> _messages = new List<string>();
        private float _nextLogTime;
        private float _lastPoseTime = -10f; // NUEVO

        private void OnEnable() => PoseLandmarkListAnnotation.Drawn += OnPoseDrawn;
        private void OnDisable() => PoseLandmarkListAnnotation.Drawn -= OnPoseDrawn;

        private void OnPoseDrawn(PoseLandmarkListAnnotation annotation)
        {
            var landmarks = annotation.Points;
            if (landmarks.count < Count || !landmarks[LeftShoulder].isActive) return;

            var screen = landmarks.GetScreenRect();
            if (screen.width <= 0f || screen.height <= 0f) return;

            // Posiciones en 0-1 respecto a la imagen (pueden salir de ese rango si la parte está fuera)
            for (int i = 0; i < Count; i++)
            {
                Vector2 p = landmarks[i].transform.localPosition;
                _points[i] = new Vector2((p.x - screen.xMin) / screen.width, (p.y - screen.yMin) / screen.height);
            }

            LastReport = FramingAnalyzer.Analyze(_points, _settings);
            FramingGuide.Advise(LastReport, _settings, _messages); // NUEVO: se calcula siempre, para el cartel
            _lastPoseTime = Time.time;                             // NUEVO

            if (Time.time < _nextLogTime) return;
            _nextLogTime = Time.time + _logInterval;

            Debug.Log("Encuadre: " + (_messages.Count == 0 ? "correcto" : string.Join("   |   ", _messages)));
        }
    }
}