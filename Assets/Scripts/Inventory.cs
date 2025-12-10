using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private HashSet<string> keys = new HashSet<string>(); // Keys created via HashSet so each key is unique

    // AddKey(string keyID)
    // Function for when the player obtains a key to be added into the Inventory
    public void AddKey(string keyID)
    {
        if (!string.IsNullOrEmpty(keyID)) // When a key is collected
        {
            keys.Add(keyID); //Add to Inventory
            Debug.Log($"[Inventory] Added Key: {keyID}");
        }
    }
    
    // HasKey (string keyID)
    // Checks if player has correct key (for advancing through doors)
    public bool HasKey(string keyID)
    {
        if (string.IsNullOrEmpty(keyID)) return false; //If incorrect key is used for door
        return keys.Contains(keyID); //Otherwise, use key to open door
    }
}
