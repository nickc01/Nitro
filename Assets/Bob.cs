using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bob : MonoBehaviour
{
    Renderer renderer;

    public AnimationCurve curve;
    public float YAmount = 1f;

    float timer = 0f;

    float startingY = 0;

    [Tooltip("How fast the powerup rotates in degrees per second")]
    public float rotationSpeed = 0f;

    void Start()
    {
        renderer = GetComponentInChildren<Renderer>();
        timer = Random.value;
        startingY = transform.position.y;
    }

    void Update()
    {
        if (renderer.enabled)
        {
            timer += Time.deltaTime;
            if (timer > 1f)
            {
                timer -= 1f;
            }

            transform.position = new Vector3(transform.position.x, startingY + (curve.Evaluate(timer) * YAmount), transform.position.z);

            transform.rotation *= Quaternion.Euler(0f, rotationSpeed * Time.deltaTime, 0f);
        }
    }
}
