namespace BodyTracking
{
  // Cómo se miden los ángulos
  public enum AngleSpace
  {
    Screen2D, // con los puntos de la imagen: estable, coincide con lo que se ve
    World3D,  // con los puntos 3D en metros: tiene en cuenta la profundidad, tiembla más
  }
}
