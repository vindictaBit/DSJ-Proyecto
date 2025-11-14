using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerShooter3D : MonoBehaviour
{
    [Header("Referencias")]
    public Transform muzzle;
    public GameObject projectilePrefab;
    public LineRenderer aimLine;

    [Header("Parámetros")]
    public float powerMultiplier = 5f;
    public float minPower = 10f;
    public float maxPower = 30f;

    private Camera cam;
    private Vector3 dragStart;
    private bool dragging;
    private InventoryUI inventoryUI;
    private WeaponManager weaponManager;

    void Start()
    {
        cam = Camera.main;
        if (aimLine != null)
            aimLine.gameObject.SetActive(false);
        
        inventoryUI = FindObjectOfType<InventoryUI>();
        weaponManager = GetComponent<WeaponManager>();
    }

    void Update()
    {
        // ===== BLOQUEO: No disparar si inventario abierto =====
        if (inventoryUI != null && inventoryUI.IsPointerOverUI())
        {
            // Si estábamos arrastrando, cancelar
            if (dragging)
            {
                dragging = false;
                if (aimLine != null) aimLine.gameObject.SetActive(false);
            }
            return; // No procesar input de disparo
        }
        // ======================================================

        if (Input.GetMouseButtonDown(0))
        {
            dragStart = GetMousePointOnPlane();
            dragging = true;
            if (aimLine != null) aimLine.gameObject.SetActive(true);
        }

        if (dragging && Input.GetMouseButton(0))
        {
            Vector3 current = GetMousePointOnPlane();
            Vector3 dir = current - dragStart;
            DrawAimLine(dir);
        }

        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            Vector3 end = GetMousePointOnPlane();
            Vector3 dir = end - dragStart;

            Fire(dir);
            if (aimLine != null) aimLine.gameObject.SetActive(false);
        }
    }

    Vector3 GetMousePointOnPlane()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        plane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }

    void Fire(Vector3 dragVector)
    {
        // ===== VERIFICAR ARMA EQUIPADA =====
        if (weaponManager != null && !weaponManager.CanShoot())
        {
            Debug.Log("No tienes arma equipada! Abre inventario (I) y selecciona un arma");
            return;
        }
        // ====================================

        if (projectilePrefab == null || muzzle == null) return;

        if (dragVector.magnitude < 0.1f)
            dragVector = muzzle.forward;

        float power = Mathf.Clamp(dragVector.magnitude * powerMultiplier, minPower, maxPower);
        Vector3 direction = dragVector.normalized;

        GameObject proj = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);
        var p = proj.GetComponent<Projectile3D>();
        if (p != null)
        {
            p.speed = power;
            p.Launch(direction);
        }
    }

    void DrawAimLine(Vector3 dir)
    {
        if (aimLine == null) return;

        Vector3 start = muzzle.position;
        Vector3 end = start + dir.normalized * Mathf.Clamp(dir.magnitude * powerMultiplier, minPower, maxPower);

        aimLine.positionCount = 2;
        aimLine.SetPosition(0, start);
        aimLine.SetPosition(1, end);
    }
}