using Mediapipe.Unity.Sample.UI;
using UnityEngine;

namespace BodyTracking
{
    // Abre la ventana de instrucciones al darle a Play, usando el mismo Modal que las otras ventanas
    public class ShowInstructionsOnStart : MonoBehaviour
    {
        [SerializeField] private Modal _modalPanel;
        [SerializeField] private GameObject _instructionsWindow;

        private void Start()
        {
            if (_modalPanel != null && _instructionsWindow != null)
            {
                _modalPanel.Open(_instructionsWindow);
            }
        }
    }
}