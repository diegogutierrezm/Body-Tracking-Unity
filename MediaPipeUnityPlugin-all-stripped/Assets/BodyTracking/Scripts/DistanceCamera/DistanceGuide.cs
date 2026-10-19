namespace BodyTracking
{
    // Convierte la distancia en una instrucción para la persona
    public static class DistanceGuide
    {
        public static string Message(float distance, float minDistance, float maxDistance)
        {
            if (distance < minDistance)
                return $"Estás muy cerca, ALEJATE ";

            if (distance > maxDistance)
                return $"Estás muy lejos ACERCATE";

            return $" Distancia correcta";
        }
    }
}