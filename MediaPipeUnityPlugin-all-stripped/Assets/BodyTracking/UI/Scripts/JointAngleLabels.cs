using Mediapipe.Unity;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
    // Un cartel pequeño por articulación seleccionada, que sigue a su punto en pantalla
    public class JointAngleLabels : MonoBehaviour
    {
        [SerializeField] private JointAngleMeasurer _measurer;
        [SerializeField, Tooltip("Prefab del cartel (con ArticulacionUI)")] private ArticulacionUI _cartelPrefab;
        [SerializeField, Tooltip("Separación del cartel respecto a su punto")] private Vector2 _offset = new Vector2(15f, 15f);
        [SerializeField, Tooltip("Tamaño de los carteles")] private float _scale = 1f;

        private PoseLandmarkListAnnotation _annotation;
        private ArticulacionUI[] _carteles = new ArticulacionUI[0];
        private ArticulacionUI _cartelCuello;

        private void Awake()
        {
            if (_measurer == null) _measurer = FindFirstObjectByType<JointAngleMeasurer>();
        }

        private void OnEnable() => PoseLandmarkListAnnotation.Drawn += OnPoseDrawn;
        private void OnDisable() => PoseLandmarkListAnnotation.Drawn -= OnPoseDrawn;

        // La primera vez que se dibuja la pose, crea los carteles dentro de su capa
        private void OnPoseDrawn(PoseLandmarkListAnnotation annotation)
        {
            if (annotation != _annotation || _carteles.Length != _measurer.Joints.Length)
                CrearCarteles(annotation);
        }

        // Cada fotograma: muestra solo los del grupo marcado y los pone junto a su punto
        private void LateUpdate()
        {
            if (_annotation == null || _measurer == null) return;
            var points = _annotation.Points;
            if (points.count < Count) return;

            var groups = _measurer.VisibleGroups;
            var joints = _measurer.Joints;

            for (int i = 0; i < joints.Length; i++)
            {
                var joint = joints[i];
                bool visible = (groups & joint.group) != 0;
                _carteles[i].AlternarVisibilidad(visible);
                if (!visible) continue;

                var punto = Pos(points, joint.jointB);
                var cuello = (Pos(points, LeftShoulder) + Pos(points, RightShoulder)) * 0.5f;
                bool haciaLaIzquierda = punto.x < cuello.x;

                // El pivote decide hacia dónde crece el cartel: 0 = hacia la derecha, 1 = hacia la izquierda
                ((RectTransform)_carteles[i].transform).pivot = new Vector2(haciaLaIzquierda ? 1f : 0f, 0.5f);
                var offset = new Vector2(haciaLaIzquierda ? -_offset.x : _offset.x, _offset.y);
                _carteles[i].Colocar(punto + offset);
                _carteles[i].ActualizarAngulo(joint.currentAngle);
            }

            bool neckVisible = (groups & BodyGroup.Neck) != 0;
            _cartelCuello.AlternarVisibilidad(neckVisible);
            if (neckVisible)
            {
                var neck = (Pos(points, LeftShoulder) + Pos(points, RightShoulder)) * 0.5f;


                // Pivote arriba en el centro: el cartel cuelga hacia abajo, centrado bajo el punto
                ((RectTransform)_cartelCuello.transform).pivot = new Vector2(0.5f, 1f);
                _cartelCuello.Colocar(neck + new Vector2(0f, -_offset.y));
                _cartelCuello.ActualizarTexto(_measurer.NeckText);
            }
        }

        private static Vector2 Pos(PointListAnnotation points, int i) => points[i].transform.localPosition;

        private void CrearCarteles(PoseLandmarkListAnnotation annotation)
        {
            foreach (var cartel in _carteles) if (cartel != null) Destroy(cartel.gameObject);
            if (_cartelCuello != null) Destroy(_cartelCuello.gameObject);

            _annotation = annotation;
            var parent = annotation.Points.transform; // la misma capa que los puntos
            var joints = _measurer.Joints;

            _carteles = new ArticulacionUI[joints.Length];
            for (int i = 0; i < joints.Length; i++)
                _carteles[i] = Crear(parent, joints[i].jointName);

            _cartelCuello = Crear(parent, "Cuello");
        }

        private ArticulacionUI Crear(Transform parent, string nombre)
        {
            var cartel = Instantiate(_cartelPrefab, parent);
            cartel.name = "Cartel " + nombre;
            cartel.nombreArticulacion = nombre;
            cartel.transform.localScale = Vector3.one * _scale;
            cartel.AlternarVisibilidad(false);
            return cartel;
        }
    }
}