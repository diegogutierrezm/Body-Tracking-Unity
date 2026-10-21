using System;
using System.Collections.Generic;
using UnityEngine;

namespace BodyTracking
{
    [Serializable]
    public class LightingThresholds
    {
        [Range(0f, 1f)] public float minBrightness = 0.25f;
        [Range(0f, 1f)] public float maxBrightness = 0.75f;
        [Range(0f, 1f)] public float minContrast = 0.12f;
        [Range(0f, 1f), Tooltip("Máximo de imagen quemada (casi blanca)")] public float maxOverexposed = 0.08f;
        [Range(0f, 1f), Tooltip("Máximo de imagen casi negra")] public float maxUnderexposed = 0.20f;
        [Tooltip("Si el fondo es X veces más brillante que la persona, hay contraluz")] public float maxBacklightRatio = 1.1f;
    }

    // Convierte el análisis en instrucciones para la persona
    public static class LightingAdvisor
    {
        public static void Advise(in LightingReport r, LightingThresholds t, List<string> messages)
        {
            messages.Clear();

            if (r.Brightness < t.minBrightness)
                messages.Add($"Poca luz ({r.Brightness:P0}): enciende más luces o ponte frente a una ventana");
            else if (r.Brightness > t.maxBrightness)
                messages.Add($"Demasiada luz ({r.Brightness:P0}): baja las persianas o reduce la luz");

            if (r.Overexposed > t.maxOverexposed)
                messages.Add($"Luz directa: el {r.Overexposed:P0} de la imagen está quemado. Evita que el sol o un foco den en la cámara o directamente sobre ti");

            if (r.Underexposed > t.maxUnderexposed)
                messages.Add($"Sombras fuertes: el {r.Underexposed:P0} de la imagen está casi negro. Ilumina esa zona");

            if (r.Contrast < t.minContrast)
                messages.Add("Poco contraste: la imagen se ve plana. Usa ropa que contraste con el fondo y una luz de frente");

            if (r.HasPerson && r.BackgroundBrightness > r.PersonBrightness * t.maxBacklightRatio)
                messages.Add("Contraluz: el fondo está mucho más iluminado que tú. Ponte de cara a la luz, no de espaldas");

        }
    }
}