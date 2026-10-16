using UnityEngine;

namespace BodyTracking
{
  // Suaviza un array de puntos (2D o 3D) con un One Euro Filter por coordenada
  public sealed class PointSmoother
  {
    private readonly OneEuroFilter[] _x, _y, _z;
    private float _lastTime = -1f;

    public PointSmoother(int count)
    {
      _x = new OneEuroFilter[count];
      _y = new OneEuroFilter[count];
      _z = new OneEuroFilter[count];
      for (int i = 0; i < count; i++)
      {
        _x[i] = new OneEuroFilter();
        _y[i] = new OneEuroFilter();
        _z[i] = new OneEuroFilter();
      }
    }

    // Llamar cuando se pierde a la persona, para que no "arrastre" la pose anterior
    public void Reset()
    {
      _lastTime = -1f;
      for (int i = 0; i < _x.Length; i++) { _x[i].Reset(); _y[i].Reset(); _z[i].Reset(); }
    }

    public void Apply(Vector2[] points, float time, float minCutoff, float beta)
    {
      float dt = DeltaTime(time);
      for (int i = 0; i < points.Length; i++)
      {
        points[i] = new Vector2(
          _x[i].Filter(points[i].x, dt, minCutoff, beta),
          _y[i].Filter(points[i].y, dt, minCutoff, beta));
      }
    }

    public void Apply(Vector3[] points, float time, float minCutoff, float beta)
    {
      float dt = DeltaTime(time);
      for (int i = 0; i < points.Length; i++)
      {
        points[i] = new Vector3(
          _x[i].Filter(points[i].x, dt, minCutoff, beta),
          _y[i].Filter(points[i].y, dt, minCutoff, beta),
          _z[i].Filter(points[i].z, dt, minCutoff, beta));
      }
    }

    private float DeltaTime(float time)
    {
      float dt = _lastTime < 0f ? 1f / 30f : Mathf.Max(time - _lastTime, 0.001f);
      _lastTime = time;
      return dt;
    }
  }
}
