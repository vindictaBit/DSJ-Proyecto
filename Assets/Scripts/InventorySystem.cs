using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int quantity;

    public InventorySlot(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }

    public bool IsEmpty()
    {
        return item == null || quantity <= 0;
    }
}

public class InventorySystem : MonoBehaviour
{
    [Header("Configuración")]
    public int maxSlots = 8;
    
    [Header("Debug")]
    [SerializeField] private List<InventorySlot> slots;
    
    // Evento para notificar cambios en el inventario
    public delegate void OnInventoryChanged();
    public OnInventoryChanged onInventoryChanged;

    void Awake()
    {
        // Inicializar slots vacíos
        slots = new List<InventorySlot>();
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot(null, 0));
        }
    }

    // Agregar item al inventario
    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null) return false;

        // Si es stackable, buscar slot existente
        if (item.isStackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].item == item && slots[i].quantity < item.maxStack)
                {
                    int spaceLeft = item.maxStack - slots[i].quantity;
                    int amountToAdd = Mathf.Min(quantity, spaceLeft);
                    slots[i].quantity += amountToAdd;
                    quantity -= amountToAdd;
                    
                    onInventoryChanged?.Invoke();
                    
                    if (quantity <= 0) return true;
                }
            }
        }

        // Buscar slot vacío
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i].item = item;
                slots[i].quantity = quantity;
                onInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.Log("Inventario lleno!");
        return false;
    }

    // Remover item del inventario
    public bool RemoveItem(Item item, int quantity = 1)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
            {
                slots[i].quantity -= quantity;
                
                if (slots[i].quantity <= 0)
                {
                    slots[i].item = null;
                    slots[i].quantity = 0;
                }
                
                onInventoryChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    // Usar item en un slot específico
    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;
        
        InventorySlot slot = slots[slotIndex];
        if (slot.IsEmpty() || !slot.item.isUsable) return;

        slot.item.Use(gameObject);
        
        // Si es consumible, reducir cantidad
        if (slot.item.itemType == ItemType.Consumable)
        {
            RemoveItem(slot.item, 1);
        }
    }

    // Obtener todos los slots
    public List<InventorySlot> GetSlots()
    {
        return slots;
    }

    // Verificar si tiene un item
    public bool HasItem(Item item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.item == item && slot.quantity > 0)
                return true;
        }
        return false;
    }

    // Obtener cantidad de un item
    public int GetItemCount(Item item)
    {
        int count = 0;
        foreach (InventorySlot slot in slots)
        {
            if (slot.item == item)
                count += slot.quantity;
        }
        return count;
    }
}