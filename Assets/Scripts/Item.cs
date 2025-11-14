using UnityEngine;

[System.Serializable]
public enum ItemType
{
    Weapon,      // Armas
    Tool,        // Herramientas
    Consumable,  // Consumibles (botiquines, etc)
    Key          // Llaves o items especiales
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("Información Básica")]
    public string itemName = "Nuevo Item";
    public ItemType itemType;
    public Sprite icon;
    
    [Header("Propiedades")]
    [TextArea(3, 5)]
    public string description = "Descripción del item";
    public bool isStackable = false;
    public int maxStack = 1;
    
    [Header("Uso")]
    public bool isUsable = true;
    public float useTime = 1f; // Tiempo que tarda en usarse
    
    // Método virtual para que cada tipo de item pueda sobrescribirlo
    public virtual void Use(GameObject user)
    {
        Debug.Log($"{user.name} usó {itemName}");
    }
}