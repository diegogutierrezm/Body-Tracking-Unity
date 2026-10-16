using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using UnityEngine;

namespace BodyTracking
{
  // Guarda la última pose 3D. La escribe el hilo de MediaPipe y la lee Unity, por eso usa lock.
  public sealed class WorldPoseBuffer
  {
    private readonly object _lock = new object();
    private readonly Vector3[] _latest = new Vector3[PoseLandmarkIndex.Count];
    private bool _hasData;

    public void Write(IReadOnlyList<Landmark> landmarks)
    {
      lock (_lock)
      {
        _hasData = landmarks != null && landmarks.Count >= _latest.Length;
        if (!_hasData) return;

        for (int i = 0; i < _latest.Length; i++)
        {
          _latest[i] = new Vector3(landmarks[i].x, landmarks[i].y, landmarks[i].z);
        }
      }
    }

    public void Clear()
    {
      lock (_lock) _hasData = false;
    }

    // Copia la última pose en 'target'. Devuelve false si todavía no hay datos.
    public bool TryRead(Vector3[] target)
    {
      lock (_lock)
      {
        if (!_hasData) return false;
        System.Array.Copy(_latest, target, _latest.Length);
        return true;
      }
    }
  }
}
