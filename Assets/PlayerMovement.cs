using UnityEngine;

// Makes sure the player has a CharacterController
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera cam;                // The player's camera

    public float walkSpeed = 6f;      // How fast the player moves
    public float gravityForce = -9.81f; // Gravity pull strength

    public float sensitivity = 2f;    // Mouse sensitivity
    public float maxLookAngle = 80f;  // How far up/down the camera can look

    private CharacterController charCon; // Reference to CharacterController
    private Vector3 verticalVelocity;    // Stores gravity movement
    private float camPitch = 0f;         // How much the camera is looking up/down

    void Awake()
    {
        charCon = GetComponent<CharacterController>(); // Get CharacterController

        Cursor.lockState = CursorLockMode.Locked; // Lock mouse to center
        Cursor.visible = false;                  // Hide cursor
    }

    void Update()
    {
        HandleMovement(); // Move player
        HandleLook();     // Rotate camera and player
    }

    // ----------------------------
    // Moves the player with WASD
    // ----------------------------
    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal"); // A/D movement
        float z = Input.GetAxis("Vertical");   // W/S movement

        // Direction based on where the player is facing
        Vector3 moveDir = transform.right * x + transform.forward * z;

        // Move player in that direction
        charCon.Move(moveDir * walkSpeed * Time.deltaTime);

        // If touching the ground and falling, reset fall speed slightly
        if (charCon.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -1f; // Keeps player stuck to ground
        }

        // Add gravity every frame
        verticalVelocity.y += gravityForce * Time.deltaTime;

        // Apply vertical (gravity) movement
        charCon.Move(verticalVelocity * Time.deltaTime);
    }

    // ---------------------------------------
    // Handles all mouse look / camera turning
    // ---------------------------------------
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity; // Left/right mouse
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity; // Up/down mouse

        // Adjust camera pitch (up/down)
        camPitch -= mouseY;

        // Limit how far camera can tilt
        camPitch = Mathf.Clamp(camPitch, -maxLookAngle, maxLookAngle);

        // Apply pitch rotation only to camera
        cam.transform.localRotation = Quaternion.Euler(camPitch, 0f, 0f);

        // Rotate entire player left/right
        transform.Rotate(Vector3.up * mouseX);
    }
}
