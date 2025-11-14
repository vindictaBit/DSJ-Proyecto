using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [Header("Referencias")]
    public InventorySystem inventorySystem;
    public GameObject slotPrefab;
    public Transform slotsContainer;

    [Header("UI")]
    public GameObject inventoryPanel;

    [Header("Configuración")]
    public KeyCode toggleKey = KeyCode.I;
    public bool startVisible = false;

    private GameObject[] slotObjects;
    private bool isVisible;

    void Start()
    {
        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
            if (inventorySystem == null)
            {
                Debug.LogError("No se encontró InventorySystem!");
                return;
            }
        }

        if (inventoryPanel == null)
        {
            Debug.LogError("inventoryPanel no asignado en InventoryUI!");
            return;
        }

        CreateSlots();
        inventorySystem.onInventoryChanged += UpdateUI;

        isVisible = startVisible;
        inventoryPanel.SetActive(isVisible);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    void CreateSlots()
    {
        if (slotPrefab == null || slotsContainer == null)
        {
            Debug.LogError("Faltan referencias en InventoryUI!");
            return;
        }

        var slots = inventorySystem.GetSlots();
        slotObjects = new GameObject[slots.Count];

        for (int i = 0; i < slots.Count; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            slotObjects[i] = slotObj;

            Button btn = slotObj.GetComponent<Button>();
            int index = i;
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnSlotClicked(index));
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (slotObjects == null) return;

        var slots = inventorySystem.GetSlots();

        for (int i = 0; i < slotObjects.Length; i++)
        {
            UpdateSlot(slotObjects[i], slots[i]);
        }
    }

    void UpdateSlot(GameObject slotObj, InventorySlot slot)
    {
        Image iconImage = slotObj.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI quantityText = slotObj.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();

        if (slot.IsEmpty())
        {
            if (iconImage != null) iconImage.enabled = false;
            if (quantityText != null) quantityText.text = "";
        }
        else
        {
            if (iconImage != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = slot.item.icon;
            }
            if (quantityText != null)
            {
                quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
            }
        }
    }

    void OnSlotClicked(int index)
    {
        var slot = inventorySystem.GetSlots()[index];
        
        if (!slot.IsEmpty())
        {
            Debug.Log($"Equipando: {slot.item.itemName}");
            
            // Si es un arma, equiparla
            if (slot.item.itemType == ItemType.Weapon)
            {
                WeaponManager weaponMgr = inventorySystem.GetComponent<WeaponManager>();
                if (weaponMgr != null)
                {
                    weaponMgr.EquipWeapon(slot.item);
                }
            }
            else
            {
                // Si no es arma, usar normalmente
                inventorySystem.UseItem(index);
            }
        }
        else
        {
            // Slot vacío: desarmar arma actual
            Debug.Log("Slot vacío - Guardando arma");
            WeaponManager weaponMgr = inventorySystem.GetComponent<WeaponManager>();
            if (weaponMgr != null)
            {
                weaponMgr.UnequipWeapon();
            }
        }
        
        // SIEMPRE cerrar inventario al hacer clic (tenga o no item)
        HideInventory();
    }

    public void ToggleInventory()
    {
        isVisible = !isVisible;
        inventoryPanel.SetActive(isVisible);
        
        // Cambiar el cursor según estado
        UpdateCursorState();
    }

    public void ShowInventory()
    {
        isVisible = true;
        inventoryPanel.SetActive(true);
        UpdateCursorState();
    }

    public void HideInventory()
    {
        isVisible = false;
        inventoryPanel.SetActive(false);
        UpdateCursorState();
    }

    void UpdateCursorState()
    {
        if (isVisible)
        {
            // Inventario abierto: mostrar cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Inventario cerrado: cursor normal para el juego
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Verificar si el mouse está sobre la UI
    public bool IsPointerOverUI()
    {
        return isVisible && EventSystem.current.IsPointerOverGameObject();
    }

    void OnDestroy()
    {
        if (inventorySystem != null)
        {
            inventorySystem.onInventoryChanged -= UpdateUI;
        }
    }
}