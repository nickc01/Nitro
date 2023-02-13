/*using Nitro;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How fast the car accelerates")]
    public RevertableVar<float> Acceleration = 30f;
    [SerializeField]
    [Tooltip("The maximum velocity the car can travel")]
    public RevertableVar<float> TerminalVelocity = 8f;
    [SerializeField]
    [Tooltip("How fast the car rotates in degrees per second")]
    public RevertableVar<float> RotationSpeed = 90f;

    [SerializeField]
    Vector3 CenterOfMass = new Vector3(0f, 0f, 0f);

    Rigidbody body;

    HashSet<GameObject> TouchingGameObjects = new HashSet<GameObject>();

    [SerializeField]
    float airCollisionCooldown = 0.1f;

    float airCooldownTimer = 0f;

    GroundChecker groundCheck;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Player");
        body = GetComponent<Rigidbody>();
        groundCheck = GetComponent<GroundChecker>();


        body.centerOfMass = Vector3.Scale(GetBounds(gameObject).extents, CenterOfMass);
    }

    private void Update()
    {
        transform.rotation *= Quaternion.Euler(0f, Input.GetAxis("Horizontal") * RotationSpeed * Time.deltaTime, 0f);
    }

    private void FixedUpdate()
    {
        if (TouchingGameObjects.Count == 0)
        {
            airCooldownTimer += Time.fixedDeltaTime;
        }
        else
        {
            airCooldownTimer = 0;
        }

        if (groundCheck.IsTouchingGround)
        {
            Vector3 accel = transform.forward * Input.GetAxis("Vertical") * Acceleration * Time.fixedDeltaTime;

            body.velocity += accel;

            float t = TerminalVelocity;


            if (body.velocity.magnitude > t)
            {
                body.velocity = resizeVectorToLength(body.velocity, t);
            }
        }
    }

    Vector3 resizeVectorToLength(Vector3 vector, float length)
    {
        Vector3 normalized = vector.normalized;
        return new Vector3(normalized.x * length, normalized.y * length, normalized.z * length);
    }

    public static Bounds GetBounds(GameObject obj)
    {
        Bounds bounds = new Bounds();
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();

        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
            {

                if (collider.enabled)
                {
                    bounds = collider.bounds;
                    break;
                }
            }

            foreach (Collider collider in colliders)
            {
                if (collider.enabled)
                {
                    bounds.Encapsulate(collider.bounds);
                }
            }
        }
        return bounds;
    }
}*/
