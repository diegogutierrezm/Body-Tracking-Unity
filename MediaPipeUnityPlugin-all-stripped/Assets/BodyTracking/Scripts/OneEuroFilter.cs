using UnityEngine;

namespace BodyTracking
{
  // Filtro One Euro: suaviza mucho en reposo y poco en movimientos rápidos
  public sealed class OneEuroFilter
  {
    private float _value, _derivative;
    private bool _initialized;

    public void Reset() => _initialized = false;

    public float Filter(float value, float dt, float minCutoff, float beta, float dCutoff = 1f)
    {
      if (!_initialized)
      {
        _value = value;
        _derivative = 0f;
        _initialized = true;
        return value;
      }

      float derivative = (value - _value) / dt;
      _derivative = Mathf.Lerp(_derivative, derivative, Alpha(dCutoff, dt));

      float cutoff = minCutoff + beta * Mathf.Abs(_derivative);
      _value = Mathf.Lerp(_value, value, Alpha(cutoff, dt));
      return _value;
    }

    private static float Alpha(float cutoff, float dt)
    {
      float tau = 1f / (2f * Mathf.PI * cutoff);
      return 1f / (1f + tau / dt);
    }
  }
}
