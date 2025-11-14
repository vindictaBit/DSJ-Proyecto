using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Items/Weapon")]
public class WeaponItem : Item
{
    [Header("Propiedades de Arma")]
    public int damage = 10;
    public float range = 5f;
    public float fireRate = 1f;
    public int ammoCapacity = 30;
    public int currentAmmo = 30;
    
    [Header("Modelo 3D")]
    [Tooltip("Prefab o referencia del modelo 3D del arma")]
    public GameObject weaponModelPrefab;
    
    [Header("Efectos")]
    public GameObject muzzleFlashPrefab;
    public AudioClip fireSound;

    private float nextFireTime;

    public override void Use(GameObject user)
    {
        if (Time.time < nextFireTime)
        {
            Debug.Log("Arma en cooldown");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("Sin munición!");
            return;
        }

        Fire(user);
        nextFireTime = Time.time + fireRate;
    }

    void Fire(GameObject user)
    {
        currentAmmo--;
        Debug.Log($"{user.name} disparó {itemName}. Munición restante: {currentAmmo}");

        // Raycast para detectar hit
        Ray ray = new Ray(user.transform.position + Vector3.up, user.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log($"Impacto en: {hit.collider.name}");
            
            // Aquí puedes agregar lógica de daño
            // Ejemplo: hit.collider.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            
            // Efecto visual en el punto de impacto
            Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * range, Color.yellow, 1f);
        }
    }

    public void Reload(int ammo)
    {
        currentAmmo = Mathf.Min(currentAmmo + ammo, ammoCapacity);
        Debug.Log($"Recargado. Munición actual: {currentAmmo}/{ammoCapacity}");
    }

    public bool NeedsReload()
    {
        return currentAmmo < ammoCapacity;
    }
}