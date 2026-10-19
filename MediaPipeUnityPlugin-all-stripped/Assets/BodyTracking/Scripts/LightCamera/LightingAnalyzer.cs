using Unity.Collections;
using UnityEngine;

namespace BodyTracking
{
    // Resultado del análisis. Todo de 0 a 1.
    public readonly struct LightingReport
    {
        public readonly float Brightness;           // brillo medio
        public readonly float Contrast;             // desviación típica de la luminosidad
        public readonly float Overexposed;          // fracción de píxeles casi blancos
        public readonly float Underexposed;         // fracción de píxeles casi negros
        public readonly float PersonBrightness;     // -1 si no hay persona
        public readonly float BackgroundBrightness; // -1 si no hay persona

        public LightingReport(float brightness, float contrast, float overexposed, float underexposed,
                              float personBrightness, float backgroundBrightness)
        {
            Brightness = brightness;
            Contrast = contrast;
            Overexposed = overexposed;
            Underexposed = underexposed;
            PersonBrightness = personBrightness;
            BackgroundBrightness = backgroundBrightness;
        }

        public bool HasPerson => PersonBrightness >= 0f && BackgroundBrightness >= 0f;
    }

    public static class LightingAnalyzer
    {
        private const float White = 0.88f;
        private const float Black = 0.05f;

        // personRect: zona de la persona en coordenadas 0-1 (origen abajo a la izquierda). Rect.zero = sin persona
        public static LightingReport Analyze(NativeArray<Color32> pixels, int width, int height, Rect personRect)
        {
            bool hasPerson = personRect.width > 0f && personRect.height > 0f;
            double sum = 0, sumSq = 0, personSum = 0, backgroundSum = 0;
            int over = 0, under = 0, personCount = 0, backgroundCount = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var c = pixels[y * width + x];
                    float l = (0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b) / 255f;

                    sum += l;
                    sumSq += l * l;
                    if (l > White) over++;
                    else if (l < Black) under++;

                    if (!hasPerson) continue;
                    var uv = new Vector2((x + 0.5f) / width, (y + 0.5f) / height);
                    if (personRect.Contains(uv)) { personSum += l; personCount++; }
                    else { backgroundSum += l; backgroundCount++; }
                }
            }

            int n = width * height;
            float mean = (float)(sum / n);
            float contrast = Mathf.Sqrt(Mathf.Max(0f, (float)(sumSq / n) - mean * mean));
            float person = personCount > 0 ? (float)(personSum / personCount) : -1f;
            float background = backgroundCount > 0 ? (float)(backgroundSum / backgroundCount) : -1f;

            return new LightingReport(mean, contrast, (float)over / n, (float)under / n, person, background);
        }
    }
}