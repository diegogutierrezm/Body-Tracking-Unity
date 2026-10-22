using System.Collections.Generic;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using Unity.Collections;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
    // Analiza la iluminación cada pocos segundos y da instrucciones
    public class LightingMonitor : MonoBehaviour
    {
        [SerializeField] private LightingThresholds _thresholds = new LightingThresholds();
        [SerializeField, Tooltip("Segundos entre cada análisis")] private float _interval = 2f;
        [SerializeField, Range(16, 256), Tooltip("Ancho de la copia que se analiza")] private int _sampleWidth = 64;
        [SerializeField, Tooltip("Marcar si la zona de la cara se mide en el lado contrario de la imagen")]
        private bool _flipPersonX = true;

        public LightingReport LastReport { get; private set; }

        // NUEVO: lo que hay que corregir. Si está vacía, la iluminación es correcta
        public IReadOnlyList<string> Messages => _messages;

        private FrameSampler _sampler;
        private readonly List<string> _messages = new List<string>();
        private Rect _personRect;     // zona de la cara en 0-1
        private float _personRectTime = -10f;
        private float _nextTime;

        private void Awake() => _sampler = new FrameSampler(_sampleWidth);
        private void OnDestroy() => _sampler?.Dispose();

        private void OnEnable() => PoseLandmarkListAnnotation.Drawn += OnPoseDrawn;
        private void OnDisable() => PoseLandmarkListAnnotation.Drawn -= OnPoseDrawn;

        private void Update()
        {
            if (Time.time < _nextTime) return;

            var source = ImageSourceProvider.ImageSource;
            if (source == null || !source.isPrepared) return;

            _nextTime = Time.time + _interval;
            _sampler.Request(source.GetCurrentTexture(), OnFrameReady);
        }

        private void OnFrameReady(NativeArray<Color32> pixels, int width, int height)
        {
            // Si hace más de medio segundo que no se detecta a nadie, se analiza sin persona
            var personRect = Time.time - _personRectTime < 0.5f ? _personRect : Rect.zero;

            LastReport = LightingAnalyzer.Analyze(pixels, width, height, personRect);
            LightingAdvisor.Advise(LastReport, _thresholds, _messages);

        }

        // Guarda la zona de la cara (puntos 0-10): la piel es la mejor referencia para saber si hay contraluz
        private void OnPoseDrawn(PoseLandmarkListAnnotation annotation)
        {
            var landmarks = annotation.Points;
            if (landmarks.count < Count || !landmarks[LeftShoulder].isActive) return;

            var screen = landmarks.GetScreenRect();
            float minX = 1f, minY = 1f, maxX = 0f, maxY = 0f;

            for (int i = 0; i <= MouthRight; i++) // puntos de la cara: 0 a 10
            {
                Vector2 p = landmarks[i].transform.localPosition;
                float x = Mathf.Clamp01((p.x - screen.xMin) / screen.width);
                if (_flipPersonX) x = 1f - x;
                float y = Mathf.Clamp01((p.y - screen.yMin) / screen.height);
                minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
            }

            // Se estrecha un 15 % por cada lado, para no coger pelo, cascos ni fondo junto a las orejas
            float margin = (maxX - minX) * 0.15f;
            minX += margin;
            maxX -= margin;

            if (maxX - minX < 0.01f || maxY - minY < 0.01f) return; // cara demasiado pequeña

            _personRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            _personRectTime = Time.time;
        }
    }
}