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
        [Range(0f, 1f), Tooltip("Maximum fraction of the image that can be overexposed (almost white)")] public float maxOverexposed = 0.08f;
        [Range(0f, 1f), Tooltip("Maximum fraction of the image that can be almost black")] public float maxUnderexposed = 0.20f;
        [Tooltip("If the background is X times brighter than the person, there is backlight")] public float maxBacklightRatio = 1.1f;
    }

    // Convierte el análisis en instrucciones para la persona
    public static class LightingAdvisor
    {
        public static void Advise(in LightingReport r, LightingThresholds t, List<string> messages)
        {
            messages.Clear();

            if (r.Brightness < t.minBrightness)
                messages.Add($"Too dark ({r.Brightness:P0}): turn on more lights or face a window");
            else if (r.Brightness > t.maxBrightness)
                messages.Add($"Too bright ({r.Brightness:P0}): lower the blinds or dim the light");

            if (r.Overexposed > t.maxOverexposed)
                messages.Add($"Direct light: {r.Overexposed:P0} of the image is overexposed. Keep the sun or spotlights off the camera and off you");

            if (r.Underexposed > t.maxUnderexposed)
                messages.Add($"Strong shadows: {r.Underexposed:P0} of the image is almost black. Light up that area");

            if (r.Contrast < t.minContrast)
                messages.Add("Low contrast: the image looks flat. Wear clothes that contrast with the background and use front lighting");

            if (r.HasPerson && r.BackgroundBrightness > r.PersonBrightness * t.maxBacklightRatio)
                messages.Add("Backlight: the background is much brighter than you. Face the light, don't stand with your back to it");

        }
    }
}