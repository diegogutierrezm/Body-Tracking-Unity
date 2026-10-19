using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.Sample.UI;
using BodyTracking;

namespace Mediapipe.Unity.Sample.UI
{
    public class JointSelectionConfig : ModalContents
    {
        // Rutas relativas que buscarán tus nuevos Toggles dentro de la jerarquía
        private const string _RightArmTogglePath = "Scroll View/Viewport/Contents/RightArm/Toggle";
        private const string _LeftArmTogglePath = "Scroll View/Viewport/Contents/LeftArm/Toggle";
        private const string _RightLegTogglePath = "Scroll View/Viewport/Contents/RightLeg/Toggle";
        private const string _LeftLegTogglePath = "Scroll View/Viewport/Contents/LeftLeg/Toggle";
        private const string _NeckTogglePath = "Scroll View/Viewport/Contents/Neck/Toggle";

        private JointAngleMeasurer _jointMeasurer;
        private bool _isChanged;

        private void Start()
        {
            _jointMeasurer = FindFirstObjectByType<JointAngleMeasurer>();
            InitializeToggles();
        }

        public override void Exit()
        {
            GetModal().CloseAndResume(_isChanged);
        }

        private void InitializeToggles()
        {
            if (_jointMeasurer == null) return;

            SetupToggle(_RightArmTogglePath, BodyGroup.RightArm);
            SetupToggle(_LeftArmTogglePath, BodyGroup.LeftArm);
            SetupToggle(_RightLegTogglePath, BodyGroup.RightLeg);
            SetupToggle(_LeftLegTogglePath, BodyGroup.LeftLeg);
            SetupToggle(_NeckTogglePath, BodyGroup.Neck);
        }

        private void SetupToggle(string path, BodyGroup group)
        {
            var target = gameObject.transform.Find(path);
            if (target == null)
            {
                Debug.LogWarning($"No se encontró el Toggle en la ruta: {path}");
                return;
            }

            var toggle = target.GetComponent<Toggle>();
            if (toggle == null) return;

            toggle.onValueChanged.RemoveAllListeners();

            // Marca o desmarca la casilla según lo que esté activo actualmente en el medidor
            toggle.isOn = (_jointMeasurer.VisibleGroups & group) != 0;

            // Al hacer clic, añade o quita el grupo de manera independiente (permite selección múltiple)
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                    _jointMeasurer.VisibleGroups |= group;   // Añade el grupo a los flags
                else
                    _jointMeasurer.VisibleGroups &= ~group;  // Quita el grupo de los flags

                _isChanged = true;
            });
        }
    }
}