using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace BodyTracking
{
    // Hace una copia pequeña de la imagen y la lee desde la tarjeta gráfica sin bloquear Unity
    public sealed class FrameSampler : IDisposable
    {
        private readonly int _width;
        private RenderTexture _small;
        private bool _busy;

        public FrameSampler(int width) => _width = width;

        // onReady recibe los píxeles (solo son válidos dentro de esa llamada), el ancho y el alto
        public void Request(Texture source, Action<NativeArray<Color32>, int, int> onReady)
        {
            if (_busy || source == null || source.width == 0) return;

            int height = Mathf.Max(1, Mathf.RoundToInt(_width * (float)source.height / source.width));
            if (_small == null || _small.height != height)
            {
                Release();
                _small = new RenderTexture(_width, height, 0, RenderTextureFormat.ARGB32);
            }

            Graphics.Blit(source, _small); // copia reducida, en la GPU
            _busy = true;

            int w = _small.width, h = _small.height;
            AsyncGPUReadback.Request(_small, 0, TextureFormat.RGBA32, request =>
            {
                _busy = false;
                if (!request.hasError) onReady(request.GetData<Color32>(), w, h);
            });
        }

        public void Dispose() => Release();

        private void Release()
        {
            if (_small == null) return;
            _small.Release();
            UnityEngine.Object.Destroy(_small);
            _small = null;
        }
    }
}