using UnityEngine;

namespace BodyTracking
{
  // Dibuja una elipse con un LineRenderer
  public sealed class EllipseRenderer
  {
    private readonly LineRenderer _line;
    private readonly Vector2[] _unitCircle; // cos/sin calculados una sola vez
    private readonly Vector3[] _positions;  // array reutilizado en cada fotograma

    public bool IsStyled { get; private set; }

    public EllipseRenderer(Transform parent, string name, Color color, float width, int segments)
    {
      var go = new GameObject(name);
      go.transform.SetParent(parent, false);
      go.layer = parent.gameObject.layer;

      _line = go.AddComponent<LineRenderer>();
      _line.useWorldSpace = false;
      _line.loop = true;
      _line.startColor = _line.endColor = color;
      _line.startWidth = _line.endWidth = width;
      _line.positionCount = segments;

      _unitCircle = new Vector2[segments];
      _positions = new Vector3[segments];
      for (int i = 0; i < segments; i++)
      {
        float a = i * Mathf.PI * 2f / segments;
        _unitCircle[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
      }
    }

    // Copia material y orden de dibujado de las líneas del esqueleto
    public void CopyStyleFrom(LineRenderer template)
    {
      if (template == null) return;
      _line.sharedMaterial = template.sharedMaterial;
      _line.sortingLayerID = template.sortingLayerID;
      _line.sortingOrder = template.sortingOrder;
      IsStyled = true;
    }

    public void Draw(in HeadEllipse ellipse)
    {
      for (int i = 0; i < _positions.Length; i++)
      {
        _positions[i] = ellipse.PointAt(_unitCircle[i].x, _unitCircle[i].y);
      }
      _line.SetPositions(_positions);
    }

    public void SetVisible(bool visible)
    {
      if (_line.gameObject.activeSelf != visible) _line.gameObject.SetActive(visible);
    }
  }
}
