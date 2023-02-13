using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedCar : NetworkBehaviour
{
    Camera mainCamera;

    [SerializeField]
    Vector3 camTarget;

    [SerializeField]
    float acceleration = 5f;

    [SerializeField]
    float rotationAcceleration = 5f;

    [SerializeField]
    bool testing = false;

    [SerializeField]
    Vector3 centerOfMass = Vector3.zero;

    [SerializeField]
    float gravityMultiplier = 1;

    Rigidbody rb;

    public override void OnStartLocalPlayer()
    {
        if (testing || isLocalPlayer)
        {
            mainCamera = GameObject.FindObjectOfType<Camera>();

            rb = GetComponent<Rigidbody>();

            rb.centerOfMass = centerOfMass;
        }
    }

    private void Update()
    {
        if (testing || isLocalPlayer)
        {
            mainCamera.transform.position = transform.TransformPoint(camTarget);
            mainCamera.transform.rotation = transform.rotation;

            var horizontal = Input.GetAxis("Horizontal");

            transform.rotation *= Quaternion.Euler(0f, horizontal * rotationAcceleration * Time.deltaTime, 0f);


            var rotation = transform.rotation.eulerAngles;

            var newRotation = Quaternion.Euler(0f,rotation.y,0f);

            if (Input.GetButton("Fire1"))
            {
                rb.velocity += newRotation * (Vector3.forward * (acceleration * Time.deltaTime));
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.TransformPoint(camTarget), new Vector3(0.1f, 0.1f, 0.1f));

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.TransformPoint(centerOfMass), new Vector3(0.1f, 0.1f, 0.1f));
    }
}
