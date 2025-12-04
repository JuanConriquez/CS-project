using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(CharacterController))]
public class FirstPersonPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6f;
    public float gravity = -9.81f;

    [Header("Look")]
    public Camera playerCamera;
    public float mouseSensitivity = 120f;
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;

    [Header("Audio")]
    public AudioSource footstepSource;      
    public float walkPitch = 1.0f;
    public float runPitch = 1.2f;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraVerticalAngle = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(); //if camera is not set, automatically assign one
        }

        if (footstepSource == null)
        {
            footstepSource = GetComponent<AudioSource>();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //Lock the cursor to the middle of the screen
        Cursor.visible = false; //To hide the cursor
    }

    // Update is called once per frame
    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {

        if (Mouse.current == null) return; //If no mouse is detected, do nothing

        Vector2 mouseDelta = Mouse.current.delta.ReadValue(); //Input System, read input

        // Camera movement will be determined through mouse movement
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime; // Left/right movement via mouse
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime; // Up/down movement via mouse

        transform.Rotate(Vector3.up * mouseX); // Rotating the player to match the camera movement
        cameraVerticalAngle -= mouseY; // Vertical mouse movement to match camera movement

        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, minVerticalAngle, maxVerticalAngle); // Clamp the camera angle so player doesn't look too far up/down

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(cameraVerticalAngle, 0f, 0f); // Apply pitch rotation only to the camera so player and camera movement are independent of eachother
        }
    }

    void HandleMovement()
    {
        if (Keyboard.current == null) return; //If no keyboard connected, don't allow input to be avaliable

        //Input from WASD keys
        float inputX = 0f;
        float inputZ = 0f;

        //Read WASD keys manually
        if (Keyboard.current.aKey.isPressed) inputX -= 1f;
        if (Keyboard.current.dKey.isPressed) inputX += 1f;
        if (Keyboard.current.wKey.isPressed) inputZ += 1f;
        if (Keyboard.current.sKey.isPressed) inputZ -= 1f;

        Vector3 moveDirection = (transform.right * inputX + transform.forward * inputZ).normalized; //Convert input into world direction since movement direction is relative to where the player is facing

        bool isRunning = Keyboard.current.leftShiftKey.isPressed;
        float speed = isRunning ? runSpeed : walkSpeed; // Choose walking or running speed

        Vector3 horizontalMovement = moveDirection * speed; //Horizontal movement vector

        if (controller.isGrounded && velocity.y < 0f) //If grounded and moving downward...
        {
            velocity.y = -2f; //...reset vertical velocity to a small negative value so the controller is grounded
        }

        velocity.y += gravity * Time.deltaTime; //Apply constant gravity

        Vector3 finalVelocoty = horizontalMovement + Vector3.up * velocity.y;

        controller.Move(finalVelocoty * Time.deltaTime); //Move the character, controller handles most terrain/obsticals i.e. sliding, collision, etc...

    }

    // AUDIO
    void HandleFootsteps(bool isMovingOnGround, bool isRunning)
    {
        if (footstepSource == null) return;

        if (isMovingOnGround)
        {
            // Adjust pitch based on walk vs run 
            footstepSource.pitch = isRunning ? runPitch : walkPitch;

            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else
        {
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }
}
