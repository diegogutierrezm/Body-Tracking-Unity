using UnityEngine;
using TMPro;

public class ArticulacionUI : MonoBehaviour
{
    [Header("Componentes UI")]
    public GameObject panelCartel;
    public TextMeshProUGUI textoAngulo;

    [Header("Configuración")]
    public string nombreArticulacion = "Codo izq";

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
}
