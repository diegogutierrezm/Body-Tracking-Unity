<<<<<<< Updated upstream

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
=======
using TMPro;
using UnityEngine;

public class ArticulacionUI : MonoBehaviour
{
    [Header("Configuración de seguimiento")]
    [SerializeField] private Transform landmarkTransform; // el GameObjecto que mueve MediaPipe
    public Vector3 offset = new Vector3(0.2f, 0f, 0f); //Desplazamiento para que no tape la articulación

    [Header("Componentes UI")]
    [SerializeField] private GameObject panelCartel; // El fondo del cartel
    [SerializeField] private TextMeshProUGUI textoAngulo; // El texto UI
    [SerializeField] private string nombreArticulacion;

    private Camera camaraPrincipal;

    private void Start()
    {
        camaraPrincipal = Camera.main;
    }

    private void LateUpdate()
    {
        if (landmarkTransform == null || !panelCartel.activeSelf) return;

        //1. Mover el cartel a la posición del landmark + offset
        transform.position = landmarkTransform.position + offset;

        //2. Hacer que la UI mire siempre a la cámara (Billboarding)
        transform.LookAt(transform.position + camaraPrincipal.transform.rotation * Vector3.forward,
                         camaraPrincipal.transform.rotation * Vector3.up);
    }

    // Método público que llamará tu script actual de cálculo de ángulos
    public void ActualizarAngulo(float angulo)
    {
        if (textoAngulo != null)
        {
            textoAngulo.text = $"{nombreArticulacion}\n{angulo:F1}°";
        }
    }

    // Permite activar/desactivar visualmente este cartel desde el filtro
    public void AlternarVisibilidad(bool activar)
    {
        if (panelCartel != null)
        {
            panelCartel.SetActive(activar);
        }
>>>>>>> Stashed changes
    }
}
