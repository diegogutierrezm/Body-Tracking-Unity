using TMPro;
using UnityEngine;

namespace BodyTracking
{
    // Cartel con un título opcional y un texto. Se muestra u oculta desde otros scripts.
    public class InstructionsPosterUI : MonoBehaviour
    {
        [SerializeField, Tooltip("The poster background. If empty, this same object is used")]
        private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField, Tooltip("Shown in bold above the text. Empty = no title")]
        private string _title = "Adjust before measuring";

        [Header("Sound when it appears")]
        [SerializeField, Tooltip("Must be on an object that is never hidden (e.g. Setup Checks)")]
        private AudioSource _audioSource;
        [SerializeField] private AudioClip _appearSound;
        [SerializeField, Tooltip("Minimum seconds between two sounds")]
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