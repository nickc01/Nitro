using Assets;
using Mirror;
using Nitro;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collidable))]
public class Puddle : NetworkBehaviour
{
    private ModifierCollection modifiers = new ModifierCollection();

    [SerializeField]
    private float dragMultiplier = 2f;

    [SerializeField]
    private Vector3 spawnOffset;

    [SerializeField]
    private float puddleLifeTime = 5f;

    [SerializeField]
    LayerMask mask;

    /*private void Awake()
    {
        if (NetworkServer.active)
        {
            
        }
    }*/

    private void Awake()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
    }

    public override void OnStartServer()
    {
        var collidable = GetComponent<Collidable>();
        collidable.OnCollideStart += OnCollideStart;
        collidable.OnCollideStop += OnCollideStop;

        transform.position += new Vector3(0f,0.1f,0f);

        Debug.DrawLine(transform.position, transform.position + (Vector3.down * 0.125f), Color.red, 10f);
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit info, 0.125f, mask))
        {
            //transform.position = info.point + spawnOffset;
            transform.position = info.point;
            transform.up = info.normal;
            transform.localPosition += spawnOffset;
        }
        else
        {
            transform.position -= new Vector3(0f, 0.1f, 0f);
        }

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }

        StartCoroutine(DestroyAfter(puddleLifeTime, gameObject));
        //Destroy(gameObject, puddleLifeTime);
    }

    public override void OnStartClient()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }
    }

    IEnumerator DestroyAfter(float time, GameObject obj)
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(obj);
    }

    private void OnCollideStop(Collider collider, bool destroyed)
    {
        if (NetworkServer.active && !destroyed && collider.attachedRigidbody.TryGetComponent<RollCage>(out RollCage player))
        {
            modifiers.RevertAllFor(player.Car.Manager.CarDrag);
        }
    }

    private void OnCollideStart(Collider collider)
    {
        if (NetworkServer.active && collider.attachedRigidbody.TryGetComponent<RollCage>(out RollCage player))
        {
            modifiers.Add(player.Car.Manager.CarDrag.MultiplyBy(dragMultiplier));
        }
    }
}
