using TMPro;
using UnityEngine;

namespace BodyTracking
{
    // Cartel con un título opcional y un texto. Se muestra u oculta desde otros scripts.
    public class InstructionsPosterUI : MonoBehaviour
    {
        [SerializeField, Tooltip("El fondo del cartel. Si se deja vacío, es este mismo objeto")]
        private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField, Tooltip("Se muestra en negrita encima del texto. Vacío = sin título")]
        private string _title = "Ajusta antes de medir";

        [Header("Sonido al aparecer")]
        [SerializeField, Tooltip("Debe estar en un objeto que no se oculte (por ejemplo, Solution)")]
        private AudioSource _audioSource;
        [SerializeField] private AudioClip _appearSound;
        [SerializeField, Tooltip("Segundos mínimos entre un sonido y el siguiente")]
        private float _soundCooldown = 3f;

        private float _lastSoundTime = -100f;

        private void Awake()
        {
            if (_panel == null) _panel = gameObject;
            if (_text == null) _text = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        public void Show(string message)
        {
            if (_text != null)
                _text.text = string.IsNullOrEmpty(_title) ? message : $"<b>{_title}</b>\n{message}";

            if (!_panel.activeSelf)
            {
                _panel.SetActive(true);
                PlayAppearSound(); // solo cuando pasa de oculto a visible
            }
        }

        private void PlayAppearSound()
        {
            if (_audioSource == null || _appearSound == null) return;
            if (Time.unscaledTime - _lastSoundTime < _soundCooldown) return;

            _audioSource.PlayOneShot(_appearSound);
            _lastSoundTime = Time.unscaledTime;
        }

        public void Hide()
        {
            if (_panel.activeSelf) _panel.SetActive(false);
        }
    }
}