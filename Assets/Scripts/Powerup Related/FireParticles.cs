using Mirror;
using Nitro;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Collidable))]
public class FireParticles : NetworkBehaviour
{
    [HideInInspector]
    public FirePowerup SourcePowerup;
    private ModifierCollection modifiers = new ModifierCollection();

    [SerializeField]
    float lifeTime;

    public override void OnStartServer()
    {
        var collidable = GetComponent<Collidable>();
        collidable.OnCollideStart += OnCollideStart;
        collidable.OnCollideStop += OnCollideStop;

        StartCoroutine(MainRoutine());
    }

    public override void OnStartClient()
    {
        if (!NetworkServer.active)
        {
            StartCoroutine(MainRoutine());
        }
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
            modifiers.Add(player.Car.Manager.CarDrag.MultiplyBy(SourcePowerup.dragMultiplier));
        }
    }

    private void OnParticleSystemStopped()
    {
        if (NetworkServer.active)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    public IEnumerator MainRoutine()
    {
        yield return new WaitForSeconds(lifeTime);

        var particleSystems = GetComponentsInChildren<ParticleSystem>();

        foreach (var particle in particleSystems)
        {
            particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }

        GetComponent<Collider>().enabled = false;
        //particleInstance.Stop();

        yield return new WaitForSeconds(2f);
        if (NetworkServer.active)
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}
