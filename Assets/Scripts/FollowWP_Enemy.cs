using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowWP_Enemy : MonoBehaviour
{
    [Header("Patrulla (Waypoints)")]
    public GameObject[] waypoints;
    private int currentWP = 0;
    public float speed = 5.0f;
    public float rotSpeed = 5.0f;

    [Header("Persecución del Jugador")]
    public float detectionRange = 10f;   // Distancia de detección del jugador
    public float stopDistance = 2f;      // Distancia mínima al jugador
    public string playerTag = "Player";

    private Transform player;
    private bool playerDetected = false;
    private float fixedY; // altura fija del enemigo

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag)?.transform;

        if (waypoints.Length == 0)
            Debug.LogWarning("⚠️ No hay waypoints asignados al enemigo " + gameObject.name);

        // Guardar la altura inicial del enemigo
        fixedY = transform.position.y;
    }

    void Update()
    {
        if (player == null || waypoints.Length == 0)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Si el jugador entra al rango → perseguir
        if (distanceToPlayer <= detectionRange)
        {
            playerDetected = true;
        }
        // Si el jugador sale del rango con un pequeño margen → volver a patrullar
        else if (distanceToPlayer > detectionRange * 1.2f)
        {
            playerDetected = false;
        }

        if (playerDetected)
        {
            FollowPlayer(distanceToPlayer);
        }
        else
        {
            Patrol();
        }
    }

    // ------------------ PATRULLA ------------------
    void Patrol()
    {
        Vector3 targetPos = waypoints[currentWP].transform.position;
        MoveTowards(targetPos);

        // Cuando llega cerca del waypoint, pasa al siguiente
        if (Vector3.Distance(transform.position, targetPos) < 2f)
        {
            currentWP++;
            if (currentWP >= waypoints.Length)
                currentWP = 0;
        }
    }

    // ------------------ PERSEGUIR JUGADOR ------------------
    void FollowPlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > stopDistance)
        {
            MoveTowards(player.position);
        }
        else
        {
            // Aquí puedes agregar animación de ataque o idle
        }
    }

    // ------------------ MOVIMIENTO GENERAL ------------------
    void MoveTowards(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            // Rotación más suave hacia el objetivo
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotSpeed * Time.deltaTime * 0.5f // 👈 más suave
            );

            // Movimiento suave hacia el frente (ajustado a la dirección)
            Vector3 moveDir = Vector3.Lerp(transform.forward, direction.normalized, Time.deltaTime * 2f);
            transform.position += moveDir * speed * Time.deltaTime;

            // Mantener altura fija
            transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);
        }
    }

    // ------------------ VISUALIZACIÓN ------------------
    private void OnDrawGizmosSelected()
    {
        // Rango de detección del jugador
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Waypoints
        Gizmos.color = Color.cyan;
        if (waypoints != null)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].transform.position, 0.3f);
                    if (i + 1 < waypoints.Length)
                        Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position);
                }
            }
        }
    }
}
