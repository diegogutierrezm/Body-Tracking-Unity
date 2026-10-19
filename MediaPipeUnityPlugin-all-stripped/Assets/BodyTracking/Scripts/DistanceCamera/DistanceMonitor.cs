using Mediapipe.Unity;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
    // Calcula a qué distancia está la persona y le dice si debe acercarse o alejarse
    public class DistanceMonitor : MonoBehaviour
    {
        [Header("Rango ideal (metros)")]
        [SerializeField] private float _minDistance;
        [SerializeField] private float _maxDistance;

        [Header("Estimación")]
        [SerializeField] private DistanceSettings _settings = new DistanceSettings();

        [Header("Calibración")]
        [SerializeField, Tooltip("Distancia (medida con cinta) a la que te pones al calibrar")]
        private float _calibrationDistance = 2f;
        [SerializeField] private KeyCode _calibrationKey = KeyCode.C;

        [Header("Console")]
        [SerializeField, Tooltip("Segundos entre cada mensaje")] private float _logInterval = 1f;
        [SerializeField, Range(0.01f, 1f), Tooltip("Más bajo = más suave pero más lento")]
        private float _smoothing = 0.2f;

        public float Distance { get; private set; }

        private const string FocalKey = "BodyTracking.CalibratedFocal";

        private readonly DistanceEstimator _estimator = new DistanceEstimator();
        private float _shoulderWidth, _torsoLength; // en anchos de imagen
        private bool _hasPerson;
        private float _nextLogTime;

        private void Awake()
        {
            // Recupera la calibración guardada de otras veces
            if (PlayerPrefs.HasKey(FocalKey)) _estimator.CalibratedFocal = PlayerPrefs.GetFloat(FocalKey);
        }

        private void OnEnable() => PoseLandmarkListAnnotation.Drawn += OnPoseDrawn;
        private void OnDisable() => PoseLandmarkListAnnotation.Drawn -= OnPoseDrawn;

        private void Update()
        {
            if (Input.GetKeyDown(_calibrationKey)) Calibrate();
        }

        // También se puede lanzar desde el Inspector: los tres puntos del componente → Calibrar
        [ContextMenu("Calibrar")]
        public void Calibrate()
        {
            if (!_hasPerson)
            {
                Debug.LogWarning("No se puede calibrar: no hay nadie delante de la cámara");
                return;
            }

            _estimator.Calibrate(_calibrationDistance, _shoulderWidth, _torsoLength, _settings);
            PlayerPrefs.SetFloat(FocalKey, _estimator.CalibratedFocal.Value);
            Distance = _calibrationDistance;
            Debug.Log($"Calibrado a {_calibrationDistance:F1} m");
        }

        [ContextMenu("Borrar calibración")]
        public void ClearCalibration()
        {
            _estimator.CalibratedFocal = null;
            PlayerPrefs.DeleteKey(FocalKey);
            Debug.Log("Calibración borrada: se usará el campo de visión");
        }

        private void OnPoseDrawn(PoseLandmarkListAnnotation annotation)
        {
            var landmarks = annotation.Points;
            if (landmarks.count < Count) return;

            _hasPerson = landmarks[LeftShoulder].isActive && landmarks[RightShoulder].isActive;
            if (!_hasPerson) return;

            float imageWidth = landmarks.GetScreenRect().width;
            if (imageWidth <= 0f) return;

            Vector2 P(int i) => landmarks[i].transform.localPosition;

            var shoulderMid = (P(LeftShoulder) + P(RightShoulder)) * 0.5f;
            var hipMid = (P(LeftHip) + P(RightHip)) * 0.5f;
            _shoulderWidth = Vector2.Distance(P(LeftShoulder), P(RightShoulder)) / imageWidth;
            _torsoLength = Vector2.Distance(shoulderMid, hipMid) / imageWidth;

            float distance = _estimator.Estimate(_shoulderWidth, _torsoLength, _settings);
            Distance = Distance <= 0f ? distance : Mathf.Lerp(Distance, distance, _smoothing);

            if (Time.time < _nextLogTime) return;
            _nextLogTime = Time.time + _logInterval;

            string message = DistanceGuide.Message(Distance, _minDistance, _maxDistance);
            if (!_estimator.IsCalibrated)
            {
                Debug.Log(message);
            }
        }
    }
}