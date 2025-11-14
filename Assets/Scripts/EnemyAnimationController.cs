using UnityEngine;

/// <summary>
/// Controla las animaciones del enemigo según su movimiento real.
/// Usa las animaciones Idle y Run automáticamente.
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimatorController : MonoBehaviour
{
    private Animator animator;
    private FollowWP_Enemy enemy;
    private Vector3 lastPosition;
    private float movementSpeed;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<FollowWP_Enemy>();

        if (animator == null)
            Debug.LogWarning("⚠️ No hay Animator en " + gameObject.name);

        lastPosition = transform.position;
    }

    void Update()
    {
        // Detecta velocidad real (basado en movimiento entre frames)
        movementSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;

        // Si se mueve más de 0.05 → correr
        bool isRunning = movementSpeed > 0.05f;

        animator.SetBool("isRunning", isRunning);

        lastPosition = transform.position;
    }
}
