using UnityEngine;
using TMPro;

public class ArticulacionUI : MonoBehaviour
{
    [Header("Componentes UI")]
    public GameObject panelCartel;
    public TextMeshProUGUI textoAngulo;

    [Header("Configuración")]
    public string nombreArticulacion;

    private void Awake()
    {
        if (panelCartel == null)
            panelCartel = gameObject;

        if (textoAngulo == null)
            textoAngulo = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void ActualizarAngulo(float angulo)
    {
        if (textoAngulo != null)
            textoAngulo.text = $"{nombreArticulacion}\n{angulo:F1}°";
    }

    public void AlternarVisibilidad(bool activar)
    {
        if (panelCartel != null)
            panelCartel.SetActive(activar);
    }

    // Coloca el cartel (en las mismas coordenadas que los puntos de la pose)
    public void Colocar(Vector2 posicion)
    {
        transform.localPosition = new Vector3(posicion.x, posicion.y, 0f);
    }

    // Para mostrar texto en vez de número (por ejemplo, el cuello: "12º derecha")
    public void ActualizarTexto(string valor)
    {
        if (textoAngulo != null)
            textoAngulo.text = $"{nombreArticulacion}\n{valor}";
    }
}
