using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // Jugador a seguir
    public float smoothSpeed = 0.125f;
    private Vector3 offset;         // Diferencia de posición inicial

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow: No se ha asignado el target.");
            enabled = false;
            return;
        }
        // Calcula la diferencia inicial (offset)
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // La rotación queda fija (la que ya tenga la cámara en escena)
    }
}
