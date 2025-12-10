using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    //Key object identifier
    //Whatever door requires a key must match the exact ID to open it
    [Tooltip("ID of the key. Must match the Door's requiredKeyID.")]
    public string keyID = "RoomA";

    // Pickup(Inventory inventory)
    // Adds key to players inventory, storing the keys the player picks up
    public void Pickup(Inventory inventory)
    {
        if (inventory == null) //Check for inventory 
        {
            Debug.LogWarning("[KeyPickup] No inventory passed in.");
            return;
        }

        inventory.AddKey(keyID); //When key is picked up, record its keyID (Adds key to inventory)
        Debug.Log($"[KeyPickup] Picked up key: {keyID}");

        Destroy(gameObject); //Make key dissapear when player interacts with it
    }
}
