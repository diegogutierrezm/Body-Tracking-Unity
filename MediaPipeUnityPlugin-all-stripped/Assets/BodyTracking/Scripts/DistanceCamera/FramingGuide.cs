using System.Collections.Generic;

namespace BodyTracking
{
    // Convierte el encuadre en instrucciones para la persona
    public static class FramingGuide
    {
        public static void Advise(in FramingReport r, FramingSettings s, List<string> messages)
        {
            messages.Clear();

            if (r.CutTop && r.CutBottom) messages.Add("Aléjate: se cortan la cabeza y los pies");
            else if (r.CutTop) messages.Add("Aléjate: se corta la cabeza");
            else if (r.CutBottom) messages.Add("Aléjate: no se ven los pies");

            if (r.CutLeft && r.CutRight) messages.Add("Aléjate: te sales por los dos lados");
            else if (r.CutLeft) messages.Add("Te sales por la izquierda de la pantalla: muévete hacia el centro");
            else if (r.CutRight) messages.Add("Te sales por la derecha de la pantalla: muévete hacia el centro");

            if (r.FullBodyVisible && r.BodyHeight < s.minBodyHeight)
                messages.Add($"Acércate: ocupas solo el {r.BodyHeight:P0} de la imagen");

            if (messages.Count == 0) messages.Add("Distancia correcta");
        }
    }
}