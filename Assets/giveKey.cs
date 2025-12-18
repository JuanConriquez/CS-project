using UnityEngine;

public class GiveKey : MonoBehaviour
{
    public string keyID = "RoomA";

    bool hasGivenKey = false;

    // Called by Interactor press E on this book
    public void Interact(Inventory inventory)
    {
        if (hasGivenKey) return;

        if (inventory == null)
        {
            Debug.LogWarning("[GiveKey] No inventory passed in.");
            return;
        }

        // Give the key directly to the player's inventory
        inventory.AddKey(keyID);
        Debug.Log($"[GiveKey] Gave key: {keyID}");

        hasGivenKey = true;

        //get rid of book as a visual cue that u did it
        Destroy(gameObject);
    }
}