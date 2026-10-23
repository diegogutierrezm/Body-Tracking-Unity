using System.Collections.Generic;

namespace BodyTracking
{
    // Convierte el encuadre en instrucciones para la persona
    public static class FramingGuide
    {
        public static void Advise(in FramingReport r, FramingSettings s, List<string> messages)
        {
            messages.Clear();

            if (r.CutTop && r.CutBottom) messages.Add("Step back: your head and feet are cut off");
            else if (r.CutTop) messages.Add("Step back: your head is cut off");
            else if (r.CutBottom) messages.Add("Step back: your feet are not visible");

            if (r.CutLeft && r.CutRight) messages.Add("Step back: you are out of frame on both sides");
            else if (r.CutLeft) messages.Add("You are out of frame on the left of the screen: move to the center");
            else if (r.CutRight) messages.Add("You are out of frame on the right of the screen: move to the center");

            if (r.FullBodyVisible && r.BodyHeight < s.minBodyHeight)
                messages.Add($"Move closer: you only fill {r.BodyHeight:P0} of the image");

        }
    }
}