using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterController))]
public class FootstepAudio : MonoBehaviour
{
    [Header("Footstep Sounds")]
    public AudioClip[] footstepClips;

    [Header("Timing")]
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.35f;

    [Header("Detection")]
    public float movementThreshold = 0.1f;

    private AudioSource audioSource;
    private CharacterController controller;
    private float stepTimer;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!controller.isGrounded) return;

        // Only check horizontal movement
        Vector3 horizontalVelocity = new Vector3(
            controller.velocity.x,
            0f,
            controller.velocity.z
        );

        if (horizontalVelocity.magnitude > movementThreshold)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float interval = isRunning ? runStepInterval : walkStepInterval;

            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = interval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(
            footstepClips[Random.Range(0, footstepClips.Length)]
        );
    }
}
