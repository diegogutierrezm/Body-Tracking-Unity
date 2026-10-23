using System.Collections;
using Mediapipe.Unity.Sample.UI;
using UnityEngine;

namespace BodyTracking
{
    // Abre las ventanas de instrucciones al darle a Play, una detrás de otra
    public class ShowInstructionsOnStart : MonoBehaviour
    {
        [SerializeField] private Modal _modalPanel;
        [SerializeField, Tooltip("Shown in this order")] private GameObject[] _windows;

        private IEnumerator Start()
        {
            if (_modalPanel == null) yield break;

            foreach (var window in _windows)
            {
                if (window == null) continue;

                _modalPanel.Open(window);

                // Espera a que se pulse Continuar (Modal.Close desactiva el panel)
                yield return new WaitWhile(() => _modalPanel.gameObject.activeSelf);
            }
        }
    }
}