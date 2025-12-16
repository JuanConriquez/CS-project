using UnityEngine;

public class Door : MonoBehaviour
{
    // Locked Door settings
    // requiresKey: Variable for when door requires a key (defualt to false to allow all doors to be open)
    // requiredKeyID: To allow specific keys to open specific doors
    [Header("Lock Settings")]
    public bool requiresKey = false; 
    public string requiredKeyID = "RoomA"; 

    // Door transform animation to simulate door opening
    // openAngle: how far door opens
    // openSpeed: how fast door opens
    [Header("Animation")]
    [Tooltip("Transform that rotates when the door opens. Default to this transform.")]
    public Transform doorTransform; 
    public float openAngle = 90f; 
    public float openSpeed = 3f; 

    private bool isOpen = false; //tracks open doors
    private Quaternion closedRotation; 
    private Quaternion openRotation;

    void Awake()
    {
        if (doorTransform == null) //Use doors own transform if no transform is provided
        {
            doorTransform = transform;
        }

        closedRotation = doorTransform.localRotation; //Store closed rotation as starting orientation
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f); //Calculate what open rotation should be
    }

    // TryOpenOrClose(Inventory inventory)
    // Function for opening/closing doors
    // Later to be integrated in Interactor (where player can interact with objects)
    public void TryOpenOrClose(Inventory inventory)
    {
        if (requiresKey) //Check if player has correct key
        {
            if (inventory == null || !inventory.HasKey(requiredKeyID)) //If player has no inventory or wrong key...
            {
                Debug.Log($"[Door] Locked. Requires key: {requiredKeyID}"); //...Door remains locked and closed
                return;
            }
        }

        //Toggle open/closed state for doors
        isOpen = !isOpen;
        Debug.Log($"[Door] {(isOpen ? "Opening" : "Closing")} door.");
    }

    void Update()
    {
        // Juan - added this so i could open and test it opens when i click E
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryOpenOrClose(null); // no inventory needed
        }
    


    Quaternion targetRot = isOpen ? openRotation : closedRotation; //Figure out where the door should be depending on the state
        // If door is opening, openRotation
        // If door is closing, closedRotation

        // Create "swinging" animation i.e. swing open or swing closed
        doorTransform.localRotation = Quaternion.Slerp(
            doorTransform.localRotation,
            targetRot,
            Time.deltaTime * openSpeed);
    }
}
