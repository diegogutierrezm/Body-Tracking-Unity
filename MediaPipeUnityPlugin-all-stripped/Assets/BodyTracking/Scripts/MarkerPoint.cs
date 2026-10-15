using Mediapipe.Unity;
using UnityEngine;

namespace BodyTracking
{
  // Punto extra (cuello, coronilla) con el mismo aspecto que los del plugin
  public sealed class MarkerPoint
  {
    private readonly PointAnnotation _point;
    private readonly Transform _transform;

    public MarkerPoint(PointAnnotation template, string name, Color color)
    {
      _point = Object.Instantiate(template, template.transform.parent);
      _point.name = name;
      _point.SetColor(color);
      _transform = _point.transform;
    }

    public void Draw(Vector2 position, bool visible)
    {
      _point.SetActive(visible);
      if (visible) _transform.localPosition = position;
    }
  }
}
