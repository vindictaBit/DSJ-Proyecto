using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleEnemy : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float detectionRange = 10f;
    public float stopDistance = 1f; // Distancia mínima para quedarse quieto

    [Header("Patrulla y Wander")]
    public Transform[] patrolPoints;
    public float wanderRadius = 5f;
    public float wanderInterval = 3f;

    private int currentPoint = 0;
    private GameObject player;
    private Vector3 wanderTarget;
    private float wanderTimer;
    private Rigidbody rb;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();

        // Configuración Rigidbody
        rb.isKinematic = true; // No se empujan entre sí
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        wanderTimer = wanderInterval;
        wanderTarget = transform.position;

        if (patrolPoints.Length == 0)
            Debug.LogWarning("No hay puntos de patrulla asignados.");
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= detectionRange)
        {
            // PERSEGUIR hasta alcanzar stopDistance
            if (distanceToPlayer > stopDistance)
                MoveTowards(player.transform.position);
            // Si ya está cerca, se queda quieto
        }
        else if (patrolPoints.Length > 0)
        {
            Patrol();
        }
        else
        {
            Wander();
        }
    }

    void Patrol()
    {
        Transform targetPoint = patrolPoints[currentPoint];
        MoveTowards(targetPoint.position);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.5f)
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }

    void Wander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
            wanderTarget = transform.position + new Vector3(randomDir.x, 0, randomDir.y);
            wanderTimer = wanderInterval;
        }

        MoveTowards(wanderTarget);
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 targetPos = new Vector3(target.x, transform.position.y, target.z);
        Vector3 direction = targetPos - rb.position;

        if (direction.sqrMagnitude > 0.01f)
        {
            Vector3 move = direction.normalized * moveSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + move);

            Quaternion rot = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, rot, 0.15f));
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
