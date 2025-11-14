using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile3D : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 20f;
    public float lifeTime = 15f;

    private Rigidbody rb;
    private bool launched = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;  // activa la caída realista
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        // La bala se destruye después de cierto tiempo si no impacta nada
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector3 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        launched = true;

        // Aplica fuerza con velocidad inicial
        rb.linearVelocity = direction.normalized * speed;

        // Rota la bala para que mire hacia donde va
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void FixedUpdate()
    {
        // Actualiza la rotación mientras se mueve (simula una bala aerodinámica)
        if (launched && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.forward = rb.linearVelocity.normalized;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Colisión con: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Impacto en enemigo!");
            Destroy(collision.gameObject); // destruye enemigo
            Destroy(gameObject);           // destruye bala
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Impacto con el suelo!");
            Destroy(gameObject, 1f); // espera 1 segundo antes de destruir
        }
        else
        {
            Destroy(gameObject, 2f); // destruye después de 2 segundos si choca con otra cosa
        }
    }
}
