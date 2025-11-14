using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("Referencias")]
    public InventorySystem inventory;
    public PlayerShooter3D shooter;
    public Transform weaponMount; // Donde se instancian/parentean las armas
    
    [Header("Estado")]
    public Item equippedWeapon; // El arma actualmente equipada
    
    private GameObject currentWeaponInstance;
    private Dictionary<Item, GameObject> weaponInstances = new Dictionary<Item, GameObject>();

    void Start()
    {
        if (inventory == null)
            inventory = GetComponent<InventorySystem>();
        
        if (shooter == null)
            shooter = GetComponent<PlayerShooter3D>();

        // Si no hay mount asignado, usar este transform
        if (weaponMount == null)
            weaponMount = this.transform;
    }

    // Llamado desde InventoryUI cuando haces clic en un arma
    public void EquipWeapon(Item weaponItem)
    {
        if (weaponItem == null)
        {
            Debug.LogWarning("Weapon es null en EquipWeapon");
            return;
        }

        // Verificar que sea un arma
        if (weaponItem.itemType != ItemType.Weapon)
        {
            Debug.LogWarning($"{weaponItem.itemName} no es un arma");
            return;
        }

        WeaponItem weapon = weaponItem as WeaponItem;
        if (weapon == null)
        {
            Debug.LogWarning($"{weaponItem.itemName} no es un WeaponItem válido");
            return;
        }

        Debug.Log($"[WeaponManager] Intentando equipar: {weapon.itemName}");

        // Verificar que el arma esté en el inventario
        if (!inventory.HasItem(weapon))
        {
            Debug.LogWarning("No tienes este arma en el inventario");
            return;
        }

        // Si ya tienes esta arma equipada, guardarla (toggle)
        if (equippedWeapon == weapon)
        {
            Debug.Log("Ya tienes esta arma equipada - Guardando");
            UnequipWeapon();
            return;
        }

        // Desactivar arma anterior
        if (currentWeaponInstance != null)
        {
            currentWeaponInstance.SetActive(false);
        }

        // Equipar nueva arma
        equippedWeapon = weapon;
        ActivateWeapon(weapon);
    }

    private void ActivateWeapon(WeaponItem weapon)
    {
        if (weapon.weaponModelPrefab == null)
        {
            Debug.LogError($"El arma '{weapon.itemName}' no tiene weaponModelPrefab asignado en el ScriptableObject!");
            return;
        }

        // Si ya existe una instancia de esta arma, reutilizarla
        if (weaponInstances.ContainsKey(weapon))
        {
            currentWeaponInstance = weaponInstances[weapon];
            currentWeaponInstance.SetActive(true);
            Debug.Log($"{weapon.itemName} activada (instancia existente)");
        }
        else
        {
            // Instanciar nueva arma preservando la transformación local del prefab
            GameObject inst = Instantiate(weapon.weaponModelPrefab, weaponMount);
            
            // IMPORTANTE: Preservar la transformación del prefab
            // Si el arma aparece mal orientada, ajusta:
            // 1. La rotación del prefab en Unity
            // 2. O la orientación del WeaponMount
            // NO modificamos position/rotation aquí para respetar el diseño del prefab
            
            inst.SetActive(true);
            
            weaponInstances[weapon] = inst;
            currentWeaponInstance = inst;
            
            Debug.Log($"{weapon.itemName} instanciada y activada");
        }
    }

    public void UnequipWeapon()
    {
        if (equippedWeapon == null)
        {
            Debug.Log("No hay arma equipada");
            return;
        }

        equippedWeapon = null;
        
        if (currentWeaponInstance != null)
        {
            currentWeaponInstance.SetActive(false);
            currentWeaponInstance = null;
        }
        
        Debug.Log("Arma guardada");
    }

    public bool CanShoot()
    {
        // Solo puede disparar si tiene un arma equipada
        return equippedWeapon != null;
    }

    // Limpiar instancias al destruir
    void OnDestroy()
    {
        foreach (var instance in weaponInstances.Values)
        {
            if (instance != null)
                Destroy(instance);
        }
        weaponInstances.Clear();
    }

    // UI en pantalla
    void OnGUI()
    {
        if (equippedWeapon != null)
        {
            GUI.Label(new Rect(Screen.width - 250, 20, 230, 40), 
                "🔫 " + equippedWeapon.itemName, 
                new GUIStyle { 
                    fontSize = 22, 
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = Color.green } 
                });
        }
        else
        {
            GUI.Label(new Rect(Screen.width - 350, 20, 330, 40), 
                "Sin arma - Abre inventario (I)", 
                new GUIStyle { 
                    fontSize = 18, 
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = new Color(1f, 0.8f, 0f) } 
                });
        }
    }
}