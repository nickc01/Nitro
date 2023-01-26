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
        //var gamepad = Gamepad.current;
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
                //transform.position += transform.forward * (acceleration * Time.deltaTime);
            }

            Debug.Log("Velocity = " + rb.velocity);

            Debug.DrawRay(transform.position, newRotation * Vector3.forward * 5f, Color.grey);


            /*var rs = gamepad.rightTrigger.ReadValue();

            var leftStick = gamepad.leftStick.ReadValue();

            Debug.Log("RS = " + rs);

            if (rs > 0.75f)
            {
                rb.velocity += (transform.forward * (acceleration * rs * Time.deltaTime));
            }

            Debug.Log("VELOCITY = " + (Vector3.forward * (acceleration * rs * Time.deltaTime)));
            Debug.Log("Left Stick X = " + leftStick.x);*/

            //rb.angularVelocity += new Vector3(0f,leftStick.x * rotationAcceleration * Time.deltaTime, 0f);
            //transform.rotation *= Quaternion.Euler(0f, leftStick.x * rotationAcceleration * Time.deltaTime, 0f);
            //rb.velocity += 
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
