using System;

namespace BodyTracking
{
  // [Flags] permite elegir varios grupos a la vez en el Inspector
  [Flags]
  public enum BodyGroup
  {
    None = 0,
    RightArm = 1 << 0,
    LeftArm = 1 << 1,
    RightLeg = 1 << 2,
    LeftLeg = 1 << 3,
    Neck = 1 << 4,
  }
}
