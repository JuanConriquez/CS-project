using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class walk : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // --- HORIZONTAL MOVEMENT (WASD / arrows) ---
        float inputX = Input.GetAxisRaw("Horizontal"); // A/D or left/right
        float inputZ = Input.GetAxisRaw("Vertical");   // W/S or up/down

        // Move relative to where the capsule is facing
        Vector3 move = transform.right * inputX + transform.forward * inputZ;
        controller.Move(move.normalized * moveSpeed * Time.deltaTime);

        // --- GRAVITY + GROUNDING ---
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            // Small negative keeps us snapped to ground
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}