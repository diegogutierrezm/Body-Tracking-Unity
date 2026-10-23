using Mediapipe.Unity;
using UnityEngine;
using static BodyTracking.PoseLandmarkIndex;

namespace BodyTracking
{
  // Coordina: recibe la pose, calcula y manda dibujar cuello, cabeza y coronilla
  public class PoseOverlay : MonoBehaviour
  {
    [Header("Head")]
    [SerializeField] private HeadEllipseSettings _head = new HeadEllipseSettings();
    [SerializeField, Range(8, 128)] private int _headSegments = 30;

    [Header("Appearance")]
    [SerializeField] private float _lineWidth = 1f;
    [SerializeField] private Color _headColor = Color.white;
    [SerializeField] private Color _neckColor = Color.magenta;
    [SerializeField] private Color _headTopColor = Color.yellow;

    [Header("Smoothing (One Euro Filter)")]
    [SerializeField, Tooltip("Lower = less jitter at rest (but more lag)")]
    private float _minCutoff = 1f;
    [SerializeField, Tooltip("Higher = less lag on fast movements (in pixels)")]
    private float _beta = 0.005f;

    public float HeadTiltAngle { get; private set; }

    private readonly Vector2[] _points = new Vector2[Count];
    private readonly PointSmoother _smoother = new PointSmoother(Count);
    private PoseLandmarkListAnnotation _annotation;
    private MarkerPoint _neck, _headTop;
    private EllipseRenderer _headEllipse;

    private void OnEnable() => PoseLandmarkListAnnotation.Drawn += OnPoseDrawn;
    private void OnDisable() => PoseLandmarkListAnnotation.Drawn -= OnPoseDrawn;

    private void OnPoseDrawn(PoseLandmarkListAnnotation annotation)
    {
      var landmarks = annotation.Points;
      if (landmarks.count < Count) return;

      if (annotation != _annotation) CreateViews(annotation);
      if (!_headEllipse.IsStyled) _headEllipse.CopyStyleFrom(annotation.SkeletonLineTemplate);

      for (int i = 0; i < Count; i++)
      {
        _points[i] = landmarks[i].transform.localPosition;
      }

      bool visible = landmarks[LeftShoulder].isActive && landmarks[RightShoulder].isActive;

      if (visible) _smoother.Apply(_points, Time.unscaledTime, _minCutoff, _beta);
      else _smoother.Reset();

      var neck = (_points[LeftShoulder] + _points[RightShoulder]) * 0.5f;
      var head = HeadEllipse.FromLandmarks(_points, _head);
      var top = head.Top;

      _neck.Draw(neck, visible);
      _headTop.Draw(top, visible);
      _headEllipse.SetVisible(visible);
      if (visible) _headEllipse.Draw(head);

      HeadTiltAngle = HeadTilt.Compute(_points[LeftShoulder], _points[RightShoulder], top);
    }

    private void CreateViews(PoseLandmarkListAnnotation annotation)
    {
      _annotation = annotation;
      var template = annotation.Points[LeftShoulder];
      _neck = new MarkerPoint(template, "Neck", _neckColor);
      _headTop = new MarkerPoint(template, "HeadTop", _headTopColor);
      _headEllipse = new EllipseRenderer(annotation.Points.transform, "HeadEllipse", _headColor, _lineWidth, _headSegments);
    }
  }
}
