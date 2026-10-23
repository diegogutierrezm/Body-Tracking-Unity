using Mediapipe.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
    // Un cartel pequeño por articulación seleccionada, que sigue a su punto en pantalla
    public class JointAngleLabels : MonoBehaviour
    {
        [SerializeField] private JointAngleMeasurer _measurer;
        [SerializeField, FormerlySerializedAs("_cartelPrefab"), Tooltip("Label prefab (with JointLabelUI)")]
        private JointLabelUI _labelPrefab;
        [SerializeField, Tooltip("Distance between the label and its point")] private Vector2 _offset = new Vector2(15f, 15f);
        [SerializeField, Tooltip("Label size")] private float _scale = 1f;

        private PoseLandmarkListAnnotation _annotation;
        private JointLabelUI[] _labels = new JointLabelUI[0];
        private JointLabelUI _neckLabel;

        private void Awake()
        {
            if (_measurer == null) _measurer = FindFirstObjectByType<JointAngleMeasurer>();
        }

        private void OnEnable() => PoseLandmarkListAnnotation.Drawn += OnPoseDrawn;
        private void OnDisable() => PoseLandmarkListAnnotation.Drawn -= OnPoseDrawn;

        // La primera vez que se dibuja la pose, crea los carteles dentro de su capa
        private void OnPoseDrawn(PoseLandmarkListAnnotation annotation)
        {
            if (annotation != _annotation || _labels.Length != _measurer.Joints.Length)
                CreateLabels(annotation);
        }

        // Cada fotograma: muestra solo los del grupo marcado y los pone junto a su punto
        private void LateUpdate()
        {
            if (_annotation == null || _measurer == null) return;
            var points = _annotation.Points;
            if (points.count < Count) return;

            var groups = _measurer.VisibleGroups;
            var joints = _measurer.Joints;
            var neck = (Pos(points, LeftShoulder) + Pos(points, RightShoulder)) * 0.5f;

            for (int i = 0; i < joints.Length; i++)
            {
                var joint = joints[i];
                bool visible = (groups & joint.group) != 0;
                _labels[i].SetVisible(visible);
                if (!visible) continue;

                var point = Pos(points, joint.jointB);
                bool towardLeft = point.x < neck.x;

                // El pivote decide hacia dónde crece el cartel: 0 = hacia la derecha, 1 = hacia la izquierda
                ((RectTransform)_labels[i].transform).pivot = new Vector2(towardLeft ? 1f : 0f, 0.5f);
                var offset = new Vector2(towardLeft ? -_offset.x : _offset.x, _offset.y);
                _labels[i].SetPosition(point + offset);
                _labels[i].SetAngle(joint.currentAngle);
            }

            bool neckVisible = (groups & BodyGroup.Neck) != 0;
            _neckLabel.SetVisible(neckVisible);
            if (neckVisible)
            {
                // Pivote arriba en el centro: el cartel cuelga hacia abajo, centrado bajo el punto
                ((RectTransform)_neckLabel.transform).pivot = new Vector2(0.5f, 1f);
                _neckLabel.SetPosition(neck + new Vector2(0f, -_offset.y));
                _neckLabel.SetValueText(_measurer.NeckText);
            }
        }

        private static Vector2 Pos(PointListAnnotation points, int i) => points[i].transform.localPosition;

        private void CreateLabels(PoseLandmarkListAnnotation annotation)
        {
            foreach (var label in _labels) if (label != null) Destroy(label.gameObject);
            if (_neckLabel != null) Destroy(_neckLabel.gameObject);

            _annotation = annotation;
            var parent = annotation.Points.transform; // la misma capa que los puntos
            var joints = _measurer.Joints;

            _labels = new JointLabelUI[joints.Length];
            for (int i = 0; i < joints.Length; i++)
                _labels[i] = CreateLabel(parent, joints[i].jointName);

            _neckLabel = CreateLabel(parent, "Neck");
        }

        private JointLabelUI CreateLabel(Transform parent, string jointName)
        {
            var label = Instantiate(_labelPrefab, parent);
            label.name = "Label " + jointName;
            label.JointName = jointName;
            label.transform.localScale = Vector3.one * _scale;
            label.SetVisible(false);
            return label;
        }
    }
}
