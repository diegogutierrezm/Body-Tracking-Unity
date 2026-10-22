using System.Collections.Generic;
using System.Text;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
    // Mide los ángulos de las articulaciones (en 2D o 3D) y escribe en la Console los del grupo seleccionado
    public class JointAngleMeasurer : MonoBehaviour
    {
        [Header("Qué se muestra")]
        [SerializeField] private BodyGroup _visibleGroups = BodyGroup.RightArm;
        [SerializeField] private AngleSpace _space = AngleSpace.Screen2D;

        [Header("Articulaciones (el ángulo se mide en B)")]
      
        [SerializeField] private JointDefinition[] _joints = JointDefinition.Defaults();

        [Header("Cuello")]
        [SerializeField] private PoseOverlay _poseOverlay;
        [SerializeField] private bool _invertNeckSides;

        [Header("Console")]
        [SerializeField, Tooltip("Segundos entre cada mensaje")] private float _logInterval = 0.5f;

        [Header("Suavizado (One Euro Filter)")]
        [SerializeField] private bool _smooth = true;
        [SerializeField, Tooltip("Más bajo = menos temblor en reposo (pero más retraso)")]
        private float _minCutoff = 1f;
        [SerializeField, Tooltip("Más alto = menos retraso en movimientos rápidos. 2D va en píxeles")]
        private float _beta2D = 0.005f;
        [SerializeField, Tooltip("Más alto = menos retraso en movimientos rápidos. 3D va en metros")]
        private float _beta3D = 0.5f;

        public BodyGroup VisibleGroups { get => _visibleGroups; set => _visibleGroups = value; }
        public AngleSpace Space { get => _space; set => _space = value; }
        public JointDefinition[] Joints => _joints;
        public float NeckAngle { get; private set; }

        // Texto del cuello: "12º derecha", "8º izq" o "0º"
        public string NeckText => TiltText.Format(NeckAngle, _invertNeckSides);
        private readonly Vector2[] _points2D = new Vector2[Count];
        private readonly Vector3[] _points3D = new Vector3[Count];
        private readonly WorldPoseBuffer _worldPose = new WorldPoseBuffer();
        private readonly StringBuilder _log = new StringBuilder();
        private readonly PointSmoother _smoother2D = new PointSmoother(Count);
        private readonly PointSmoother _smoother3D = new PointSmoother(Count);
        private float _nextLogTime;

        private void Awake()
        {
            // Si no se ha arrastrado en el Inspector, lo busca en la escena
            if (_poseOverlay == null) _poseOverlay = FindFirstObjectByType<PoseOverlay>();
        }

        private void OnEnable()
        {
            PoseLandmarkListAnnotation.Drawn += OnPoseDrawn;
            PoseLandmarkerRunner.WorldLandmarksReceived += OnWorldLandmarks;
        }

        private void OnDisable()
        {
            PoseLandmarkListAnnotation.Drawn -= OnPoseDrawn;
            PoseLandmarkerRunner.WorldLandmarksReceived -= OnWorldLandmarks;
            _worldPose.Clear();
        }

        // Llega desde el hilo de MediaPipe: solo se guarda, no se usa nada de Unity aquí
        private void OnWorldLandmarks(IReadOnlyList<Landmark> landmarks) => _worldPose.Write(landmarks);

        // Para un Dropdown de UI. Orden: Todo, Brazo der, Brazo izq, Pierna der, Pierna izq, Cuello
        public void SelectGroup(int option)
        {
            switch (option)
            {
                case 1: _visibleGroups = BodyGroup.RightArm; break;
                case 2: _visibleGroups = BodyGroup.LeftArm; break;
                case 3: _visibleGroups = BodyGroup.RightLeg; break;
                case 4: _visibleGroups = BodyGroup.LeftLeg; break;
                case 5: _visibleGroups = BodyGroup.Neck; break;
                default: _visibleGroups = (BodyGroup)~0; break; // todo
            }
        }

        // Para un Toggle de UI: false = 2D, true = 3D
        public void Select3D(bool use3D) => _space = use3D ? AngleSpace.World3D : AngleSpace.Screen2D;

        // Llega desde el hilo de Unity, cada vez que se dibuja la pose
        private void OnPoseDrawn(PoseLandmarkListAnnotation annotation)
        {
            var landmarks = annotation.Points;
            if (landmarks.count < Count) return;
            if (!landmarks[LeftShoulder].isActive || !landmarks[RightShoulder].isActive)
            {
                _smoother2D.Reset();
                _smoother3D.Reset();
                return; // no hay persona
            }

            if (!UpdateAngles(landmarks)) return;
            if (_poseOverlay != null) NeckAngle = _poseOverlay.HeadTiltAngle;


            if (Time.time < _nextLogTime) return;
            _nextLogTime = Time.time + _logInterval;
            WriteLog();
        }

        // Calcula todos los ángulos en el espacio elegido. Devuelve false si no hay datos.
        private bool UpdateAngles(PointListAnnotation landmarks)
        {
            if (_space == AngleSpace.World3D)
            {
                if (!_worldPose.TryRead(_points3D)) return false;
                if (_smooth) _smoother3D.Apply(_points3D, Time.unscaledTime, _minCutoff, _beta3D);

                foreach (var joint in _joints)
                    joint.currentAngle = JointAngle.Compute(_points3D[joint.jointA], _points3D[joint.jointB], _points3D[joint.jointC]);
            }
            else
            {
                for (int i = 0; i < Count; i++)
                    _points2D[i] = landmarks[i].transform.localPosition;
                if (_smooth) _smoother2D.Apply(_points2D, Time.unscaledTime, _minCutoff, _beta2D);

                foreach (var joint in _joints)
                    joint.currentAngle = JointAngle.Compute(_points2D[joint.jointA], _points2D[joint.jointB], _points2D[joint.jointC]);
            }
            return true;
        }

        // Escribe en la Console solo los ángulos del grupo seleccionado
        private void WriteLog()
        {
            _log.Clear();
            _log.Append(_space == AngleSpace.World3D ? "[3D]  " : "[2D]  ");

            foreach (var joint in _joints)
            {
                if ((_visibleGroups & joint.group) != 0)
                    _log.Append(joint.jointName).Append(": ").Append(joint.currentAngle.ToString("F0")).Append("º   ");
            }
            if (_poseOverlay != null && (_visibleGroups & BodyGroup.Neck) != 0)
                _log.Append("Cuello: ").Append(TiltText.Format(NeckAngle, _invertNeckSides));

            Debug.Log(_log.ToString());
        }
        public void SelectJoint(int jointIndex)
        {
            if (jointIndex >= 0 && jointIndex < _joints.Length)
            {
                _visibleGroups = _joints[jointIndex].group;
                Debug.Log($"Articulación seleccionada: {_joints[jointIndex].jointName}");
            }
        }
        // Activa o desactiva un grupo específico manteniendo los otros
        public void ToggleGroup(BodyGroup group, bool enable)
        {
            if (enable)
                _visibleGroups |= group; // Añade el grupo
            else
                _visibleGroups &= ~group; // Quita el grupo

          
        }

        // Activa todos o ninguno
        public void SetAllGroups(bool enableAll)
        {
            _visibleGroups = enableAll ? (BodyGroup)~0 : BodyGroup.None;

           
        }      
    }
}
