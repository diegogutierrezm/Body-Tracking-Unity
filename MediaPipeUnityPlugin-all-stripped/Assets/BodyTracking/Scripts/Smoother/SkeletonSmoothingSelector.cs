using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BodyTracking
{
    // Va en un Dropdown: muestra los niveles de suavizado y cambia el del SkeletonSmoother
    [RequireComponent(typeof(Dropdown))]
    public class SkeletonSmoothingSelector : MonoBehaviour
    {
        // Mismo orden que el enum SmoothingLevel: Off, Low, Medium, High
        private static readonly List<string> Options = new List<string> { "Sin suavizado", "Bajo", "Medio", "Alto" };

        private void Start()
        {
            var dropdown = GetComponent<Dropdown>();
            var smoother = FindFirstObjectByType<SkeletonSmoother>();

            if (smoother == null)
            {
                dropdown.interactable = false; // no hay SkeletonSmoother en la escena
                return;
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(Options);
            dropdown.SetValueWithoutNotify((int)smoother.Level); // muestra el nivel actual

            dropdown.onValueChanged.AddListener(value => smoother.Level = (SmoothingLevel)value);
        }
    }
}