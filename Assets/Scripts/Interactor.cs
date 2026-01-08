using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    // playerCamera: The camera the player looks through, used for raycast
    // inventory
    [Header("References")]
    public Camera playerCamera;
    public Inventory inventory;

    // interactDistance
    [Header("Interaction Settings")]
    public float interactDistance = 3f;

    void Awake()
    {
        if (inventory == null) //When no inventory is present
        {
            inventory = GetComponent<Inventory>();
        }

        if (playerCamera == null) //When no camera is assigned
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Interaction is executed by pressing the E key on the keyboard
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        if (playerCamera == null) // Ray depends on a camera in order to detect objects
        {
            Debug.LogWarning("[Interactor] No playerCamera assigned.");
            return;
        }

        // Shoot a Ray from the playerCamera
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactDistance)) //When the ray hits an object...
        {
            //juan - if ray hits a book that gives a key
            GiveKey giveKey = hitInfo.collider.GetComponent<GiveKey>();
            if (giveKey != null)
            {
                giveKey = hitInfo.collider.GetComponentInParent<GiveKey>();
            }
            if(giveKey != null)
            {
                Debug.Log($"[Interactor] Found GiveKey on: {giveKey.gameObject.name}");
                giveKey.Interact(inventory);
                return;
            }

            // If ray hits a key
            KeyPickup keyPickup = hitInfo.collider.GetComponent<KeyPickup>();
            if (keyPickup != null)
            {
                keyPickup.Pickup(inventory); //Pickup the key 
                return;
            }

            // If ray hits a door
            Door door = hitInfo.collider.GetComponent<Door>();
            if (door != null)
            {
                door.TryOpenOrClose(inventory); //Open door (if same as key)
                return;
            }
            //More interactables...
        }
    }
}