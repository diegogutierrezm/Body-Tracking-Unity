using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace BodyTracking
{
    // Cartel pequeño de una articulación: nombre + ángulo, colocado junto a su punto
    public class JointLabelUI : MonoBehaviour
    {
        // FormerlySerializedAs: conserva los valores guardados con los nombres antiguos (en español)
        [Header("UI components")]
        [SerializeField, FormerlySerializedAs("panelCartel")] private GameObject _panel;
        [SerializeField, FormerlySerializedAs("textoAngulo")] private TextMeshProUGUI _text;

        [Header("Settings")]
        [SerializeField, FormerlySerializedAs("nombreArticulacion")] private string _jointName;

        public string JointName
        {
            get => _jointName;
            set => _jointName = value;
        }

        private void Awake()
        {
            if (_panel == null)
                _panel = gameObject;

            if (_text == null)
                _text = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        public void SetAngle(float angle)
        {
            if (_text != null)
                _text.text = $"{_jointName}\n{angle:F1}°";
        }

        // Para mostrar texto en vez de número (por ejemplo, el cuello: "12º right")
        public void SetValueText(string value)
        {
            if (_text != null)
                _text.text = $"{_jointName}\n{value}";
        }

        public void SetVisible(bool visible)
        {
            if (_panel != null && _panel.activeSelf != visible)
                _panel.SetActive(visible);
        }

        // Coloca el cartel (en las mismas coordenadas que los puntos de la pose)
        public void SetPosition(Vector2 position)
        {
            transform.localPosition = new Vector3(position.x, position.y, 0f);
        }
    }
}
