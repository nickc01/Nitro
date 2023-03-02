using Mirror;
using Nitro;
using System.Collections;
using UnityEngine;

// Require a Collidable component to be attached to the game object
public class Puddle : NetworkBehaviour
{
    // A collection of Modifier objects
    private ModifierCollection modifiers = new ModifierCollection();

    // The amount to multiply the drag of any affected cars by
    [SerializeField]
    private float dragMultiplier = 2f;

    // The offset from the game object's position where it should be spawned
    [SerializeField]
    private Vector3 spawnOffset;

    // The amount of time before the game object is destroyed
    [SerializeField]
    private float puddleLifeTime = 5f;

    // The layers to collide with
    [SerializeField]
    private LayerMask mask;

    // Disable all Renderers in the game object's children during Awake
    private void Awake()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
    }

    // Called on the server when the game object is spawned
    public override void OnStartServer()
    {
        // Get the Collidable component and attach event handlers for collision events
        Collidable collidable = GetComponent<Collidable>();
        collidable.OnCollideStart += OnCollideStart;
        collidable.OnCollideStop += OnCollideStop;

        // Adjust the game object's position to be slightly above the ground and aligned with the ground's normal
        transform.position += new Vector3(0f, 0.1f, 0f);
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit info, 0.125f, mask))
        {
            transform.position = info.point;
            transform.up = info.normal;
            transform.localPosition += spawnOffset;
        }
        else
        {
            transform.position -= new Vector3(0f, 0.1f, 0f);
        }

        // Enable all Renderers in the game object's children
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }

        // Start the coroutine to destroy the game object after a set amount of time
        StartCoroutine(DestroyAfter(puddleLifeTime, gameObject));
    }

    // Called on the client when the game object is spawned
    public override void OnStartClient()
    {
        // Enable all Renderers in the game object's children
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }
    }

    // Coroutine to destroy the game object after a set amount of time
    private IEnumerator DestroyAfter(float time, GameObject obj)
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(obj);
    }

    //Called when this gameObject stops colliding with another object
    private void OnCollideStop(Collider collider, bool destroyed)
    {
        // If the game object is on the server, the collided object is not destroyed, and the collided object has a RollCage component
        if (NetworkServer.active && !destroyed && collider.attachedRigidbody.TryGetComponent<RollCage>(out RollCage player))
        {
            // Revert all modifiers applied to the player car's drag
            modifiers.RevertAllFor(player.Car.Manager.CarDrag);
        }
    }

    //Called when this gameObject collides with another object
    private void OnCollideStart(Collider collider)
    {
        // If this object is running on the server and the collider's attached rigidbody has a RollCage component
        if (NetworkServer.active && collider.attachedRigidbody.TryGetComponent<RollCage>(out RollCage player))
        {
            //Multiply the player car's drag by "dragMultiplier" and store the modification into a modifier list for later
            modifiers.Add(player.Car.Manager.CarDrag.MultiplyBy(dragMultiplier));
        }
    }
}
