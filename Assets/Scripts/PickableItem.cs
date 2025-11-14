using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("Item Configuration")]
    public Item item;
    public int quantity = 1;

    [Header("Visual")]
    public float rotationSpeed = 50f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    
    [Header("Pickup")]
    public float pickupRange = 2f;
    public KeyCode pickupKey = KeyCode.E;
    public string playerTag = "Player";

    private Vector3 startPosition;
    private GameObject nearbyPlayer;
    private bool showPrompt;

    void Start()
    {
        startPosition = transform.position;
        
        if (item == null)
        {
            Debug.LogWarning($"PickableItem en {gameObject.name} no tiene Item asignado!");
        }
    }

    void Update()
    {
        // Rotación y flotación
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Detectar jugador cercano
        CheckNearbyPlayer();
        
        // Recoger item
        if (showPrompt && nearbyPlayer != null && Input.GetKeyDown(pickupKey))
        {
            PickUp();
        }
    }

    void CheckNearbyPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            if (distance <= pickupRange)
            {
                nearbyPlayer = player;
                showPrompt = true;
            }
            else
            {
                nearbyPlayer = null;
                showPrompt = false;
            }
        }
    }

    void PickUp()
    {
        if (item == null || nearbyPlayer == null) return;

        InventorySystem inventory = nearbyPlayer.GetComponent<InventorySystem>();
        
        if (inventory != null)
        {
            bool added = inventory.AddItem(item, quantity);
            
            if (added)
            {
                Debug.Log($"Recogido: {item.itemName} x{quantity}");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventario lleno!");
            }
        }
        else
        {
            Debug.LogWarning("El jugador no tiene InventorySystem!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }

    // Para mostrar UI de prompt (opcional)
    void OnGUI()
    {
        if (showPrompt && item != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            
            if (screenPos.z > 0)
            {
                Vector2 guiPos = new Vector2(screenPos.x, Screen.height - screenPos.y - 40);
                GUI.Label(new Rect(guiPos.x - 75, guiPos.y, 150, 30), 
                    $"[{pickupKey}] Recoger {item.itemName}",
                    new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14 });
            }
        }
    }
}