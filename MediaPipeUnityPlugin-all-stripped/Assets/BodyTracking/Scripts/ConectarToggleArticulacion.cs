
using BodyTracking;
using UnityEngine;
using UnityEngine.UI;

public class ConectarToggleArticulacion : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private JointAngleMeasurer medidor;

    private void Awake()
    {
        if (toggle == null)
            toggle = GetComponent<Toggle>();
        if (toggle != null)
            Debug.Log("Toggle conectado: " + toggle.gameObject.name);
        else
            Debug.LogError("No se ha encontrado el Toggle");

        if (medidor == null)
            medidor = FindFirstObjectByType<JointAngleMeasurer>();
        Debug.Log("ConectarToggleArticulacion: Awake ejecutado");
        if (toggle != null && medidor != null)
        {
            Debug.Log("Toggle y medidor encontrados");
            toggle.onValueChanged.AddListener(
                (activar) =>
                {
                    Debug.Log("Evento Toggle recibido: " + activar);
                    medidor.SeleccionarBrazoIzquierdo(activar);
                }
            );
        }
    }

    private void OnDestroy()
    {
        if (toggle != null && medidor != null)
        {
            toggle.onValueChanged.RemoveListener(
                medidor.SeleccionarBrazoIzquierdo
            );
        }
    }
}
