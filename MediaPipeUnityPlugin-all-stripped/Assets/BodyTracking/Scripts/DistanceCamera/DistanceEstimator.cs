using System;
using UnityEngine;

namespace BodyTracking
{
    [Serializable]
    public class DistanceSettings
    {
        [Tooltip("Ancho real de hombros en metros (adulto medio ~0.38)")]
        public float shoulderWidth = 0.38f;
        [Tooltip("Largo real del tronco (centro de hombros a centro de caderas) en metros")]
        public float torsoLength = 0.50f;
        [Tooltip("Campo de visión horizontal de la webcam en grados. Solo se usa si no se ha calibrado")]
        public float horizontalFov = 70f;
    }

    // Estima la distancia a la cámara: distancia = focal * tamañoReal / tamañoEnImagen
    // Los tamaños en imagen van normalizados: 1 = todo el ancho de la imagen
    public sealed class DistanceEstimator
    {
        // Focal calibrada (en "anchos de imagen"). Si es null, se usa la del campo de visión.
        public float? CalibratedFocal { get; set; }

        public bool IsCalibrated => CalibratedFocal.HasValue;

        public float Estimate(float shoulderWidthInImage, float torsoLengthInImage, DistanceSettings s)
        {
            return Focal(s) * RealToImageRatio(shoulderWidthInImage, torsoLengthInImage, s);
        }

        // Colócate a una distancia conocida (medida con cinta métrica) y llama a esto
        public void Calibrate(float knownDistance, float shoulderWidthInImage, float torsoLengthInImage, DistanceSettings s)
        {
            CalibratedFocal = knownDistance / RealToImageRatio(shoulderWidthInImage, torsoLengthInImage, s);
        }

        private float Focal(DistanceSettings s) =>
          CalibratedFocal ?? 0.5f / Mathf.Tan(s.horizontalFov * 0.5f * Mathf.Deg2Rad);

        // Girarse estrecha los hombros e inclinarse acorta el tronco: los dos hacen parecer
        // que estás más lejos, así que nos quedamos con la medida que da más cerca
        private static float RealToImageRatio(float shoulderWidthInImage, float torsoLengthInImage, DistanceSettings s)
        {
            float byShoulders = s.shoulderWidth / Mathf.Max(shoulderWidthInImage, 0.0001f);
            float byTorso = s.torsoLength / Mathf.Max(torsoLengthInImage, 0.0001f);
            return Mathf.Min(byShoulders, byTorso);
        }
    }
}