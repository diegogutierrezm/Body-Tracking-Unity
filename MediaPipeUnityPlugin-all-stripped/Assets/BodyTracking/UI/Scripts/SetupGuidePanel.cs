using System.Text;
using UnityEngine;

namespace BodyTracking
{
    // Muestra en un cartel lo que hay que corregir (encuadre e iluminación). Si todo está bien, se oculta.
    public class SetupGuidePanel : MonoBehaviour
    {
        [SerializeField] private FramingMonitor _framing;
        [SerializeField] private LightingMonitor _lighting;
        [SerializeField, Tooltip("The InstructionsPoster in the scene")] private InstructionsPosterUI _poster;
        [SerializeField, Tooltip("Seconds between updates")] private float _refreshInterval = 0.5f;
        [SerializeField, Tooltip("Warn when nobody is in front of the camera")] private bool _warnNoPerson = true;

        private readonly StringBuilder _sb = new StringBuilder();
        private float _nextRefresh;

        private void Awake()
        {
            if (_framing == null) _framing = FindFirstObjectByType<FramingMonitor>();
            if (_lighting == null) _lighting = FindFirstObjectByType<LightingMonitor>();
        }

        private void Update()
        {
            if (_poster == null || Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + _refreshInterval;

            _sb.Clear();

            if (_framing != null)
            {
                if (!_framing.HasPerson)
                {
                    if (_warnNoPerson) Add("Stand in front of the camera");
                }
                else
                {
                    foreach (var message in _framing.Messages) Add(message);
                }
            }

            if (_lighting != null)
            {
                foreach (var message in _lighting.Messages) Add(message);
            }

            if (_sb.Length > 0) _poster.Show(_sb.ToString());
            else _poster.Hide();
        }

        private void Add(string message)
        {
            if (_sb.Length > 0) _sb.Append('\n');
            _sb.Append("• ").Append(message);
        }
    }
}