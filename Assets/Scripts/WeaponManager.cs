using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("Referencias")]
    public InventorySystem inventory;
    public PlayerShooter3D shooter;
    public Transform weaponMount; // Donde se instancian/parentean las armas
    
    [Header("Configuración de Huesos")]
    [Tooltip("Buscar automáticamente el hueso de la mano derecha")]
    public bool autoFindHandBone = true;
    
    [Tooltip("Nombre del hueso de la mano derecha (ej: 'RightHand', 'Hand_R', 'mixamorig:RightHand')")]
    public string rightHandBoneName = "RightHand";
    
    [Header("Ajuste Manual (si no usa hueso)")]
    [Tooltip("Offset de posición cuando usa WeaponMount en lugar de hueso")]
    public Vector3 weaponPositionOffset = new Vector3(0.1f, 0, 0.3f);
    public Vector3 weaponRotationOffset = new Vector3(0, 90, 0);
    
    [Header("Ajuste con Hueso de Mano")]
    [Tooltip("Offset de posición cuando usa hueso de mano")]
    public Vector3 handBonePositionOffset = Vector3.zero;
    [Tooltip("Offset de rotación cuando usa hueso de mano")]
    public Vector3 handBoneRotationOffset = new Vector3(-90, 0, 0);
    [Tooltip("Escala cuando usa hueso de mano")]
    public Vector3 handBoneScaleOffset = Vector3.one;
    
    [Header("Estado")]
    public Item equippedWeapon; // El arma actualmente equipada
    
    private GameObject currentWeaponInstance;
    private Dictionary<Item, GameObject> weaponInstances = new Dictionary<Item, GameObject>();
    private Transform rightHandBone; // Hueso de la mano derecha

    void Start()
    {
        if (inventory == null)
            inventory = GetComponent<InventorySystem>();
        
        if (shooter == null)
            shooter = GetComponent<PlayerShooter3D>();

        // Intentar encontrar el hueso de la mano derecha
        if (autoFindHandBone)
        {
            FindRightHandBone();
        }

        // Si no hay mount asignado ni hueso encontrado, usar este transform
        if (weaponMount == null && rightHandBone == null)
        {
            weaponMount = this.transform;
            Debug.LogWarning("No se encontró hueso de mano ni WeaponMount. Usando transform del jugador.");
        }
    }

    /// <summary>
    /// Busca el hueso de la mano derecha en el rig del personaje
    /// </summary>
    void FindRightHandBone()
    {
        // Buscar en todos los hijos del personaje
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        
        // Lista de nombres comunes para el hueso de la mano derecha
        string[] possibleNames = new string[] 
        { 
            rightHandBoneName,
            "RightHand", 
            "Hand_R", 
            "R_Hand",
            "mixamorig:RightHand",  // Mixamo
            "Bip001 R Hand",         // Biped
            "RightHandBone"
        };
        
        foreach (Transform child in allChildren)
        {
            foreach (string name in possibleNames)
            {
                if (child.name.Contains(name))
                {
                    rightHandBone = child;
                    Debug.Log($"✅ Hueso de mano derecha encontrado: {child.name}");
                    return;
                }
            }
        }
        
        Debug.LogWarning($"⚠️ No se encontró el hueso '{rightHandBoneName}'. Usando WeaponMount.");
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
        
        // NUEVO: Actualizar animator
        UpdateAnimatorWeaponState(true);
    }

    private void ActivateWeapon(WeaponItem weapon)
    {
        if (weapon.weaponModelPrefab == null)
        {
            Debug.LogError($"El arma '{weapon.itemName}' no tiene weaponModelPrefab asignado en el ScriptableObject!");
            return;
        }

        // Determinar dónde parentear el arma (hueso de mano o weaponMount)
        Transform parentTransform = rightHandBone != null ? rightHandBone : weaponMount;

        // Si ya existe una instancia de esta arma, reutilizarla
        if (weaponInstances.ContainsKey(weapon))
        {
            currentWeaponInstance = weaponInstances[weapon];
            currentWeaponInstance.transform.SetParent(parentTransform);
            currentWeaponInstance.SetActive(true);
            
            Debug.Log($"{weapon.itemName} activada (instancia existente) en {parentTransform.name}");
        }
        else
        {
            // Instanciar nueva arma
            GameObject inst = Instantiate(weapon.weaponModelPrefab, parentTransform);
            
            // Aplicar offset según el tipo de parent
            if (rightHandBone == null)
            {
                // Usando WeaponMount
                inst.transform.localPosition = weaponPositionOffset;
                inst.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
            }
            else
            {
                // Usando hueso de mano - Aplicar offset configurable
                inst.transform.localPosition = handBonePositionOffset;
                inst.transform.localRotation = Quaternion.Euler(handBoneRotationOffset);
            }
            
            inst.SetActive(true);
            
            weaponInstances[weapon] = inst;
            currentWeaponInstance = inst;
            
            Debug.Log($"{weapon.itemName} instanciada en {parentTransform.name}");
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
        
        // NUEVO: Actualizar animator
        UpdateAnimatorWeaponState(false);
        
        Debug.Log("Arma guardada");
    }

    /// <summary>
    /// Actualiza el parámetro hasWeapon del Animator
    /// </summary>
    private void UpdateAnimatorWeaponState(bool hasWeapon)
    {
        PersonController controller = GetComponent<PersonController>();
        if (controller != null && controller.animator != null)
        {
            controller.animator.SetBool("hasWeapon", hasWeapon);
            Debug.Log($"Animator hasWeapon = {hasWeapon}");
        }
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
        
        // DEBUG: Mostrar info de parenteo
        #if UNITY_EDITOR
        if (currentWeaponInstance != null)
        {
            string parentInfo = rightHandBone != null 
                ? $"Parented to: {rightHandBone.name}" 
                : "Using WeaponMount";
                
            GUI.Label(new Rect(10, 100, 300, 30), 
                parentInfo,
                new GUIStyle { fontSize = 12, normal = { textColor = Color.yellow } });
        }
        #endif
    }
}