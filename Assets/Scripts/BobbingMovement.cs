using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobbingMovement : MonoBehaviour
{
    // New instance of renderer component
    new Renderer renderer;

    [Tooltip("Animation curve for bobbing movement")]
    public AnimationCurve curve;

    [Tooltip("Height of bobbing movement")]
    public float YAmount = 1f;

    // Timer for controlling curve evaluation
    float timer = 0f;

    // Starting Y position
    float startingY = 0;

    [Tooltip("Rotation speed of the object in degrees per second")]
    public float rotationSpeed = 0f;

    // Start method for initializing variables
    void Start()
    {
        // Get the renderer component in children
        renderer = GetComponentInChildren<Renderer>();

        // Set timer to a random value
        timer = Random.value;

        // Store starting Y position
        startingY = transform.position.y;
    }

    // Update method for updating position and rotation
    void Update()
    {
        // Check if renderer is enabled
        if (renderer.enabled)
        {
            // Increase timer by delta time
            timer += Time.deltaTime;

            // Reset timer if it exceeds 1
            if (timer > 1f)
            {
                timer -= 1f;
            }

            // Set new position based on curve evaluation
            transform.position = new Vector3(transform.position.x, startingY + (curve.Evaluate(timer) * YAmount), transform.position.z);

            // Rotate the object by the rotation speed
            transform.rotation *= Quaternion.Euler(0f, rotationSpeed * Time.deltaTime, 0f);
        }
    }
}