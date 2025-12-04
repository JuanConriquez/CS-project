using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;        // assign the capsule here in Inspector
    public float smoothSpeed = 5f;

    private float startY;
    private float startZ;
    private float xOffset;
    private Quaternion fixedRotation;

    void Start()
    {
        // If nothing assigned, try to find the Player
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        if (target == null)
        {
            Debug.LogWarning("FollowPlayer: No target assigned and no Player found.");
            return;
        }

        // Remember starting vertical and depth position
        startY = transform.position.y;
        startZ = transform.position.z;

        // How far to the side of the player we start
        xOffset = transform.position.x - target.position.x;

        // Lock rotation
        fixedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Follow player only on X (side to side)
        float desiredX = target.position.x + xOffset;

        Vector3 desiredPosition = new Vector3(
            desiredX,
            startY,   // keep same height
            startZ    // keep same depth (no in/out)
        );

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.rotation = fixedRotation;
    }
}