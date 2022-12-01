using Nitro;
using UnityEngine;

public class FireParticles : Collidable
{
    [HideInInspector]
    public FirePowerup SourcePowerup;
    private ModifierCollection modifiers = new ModifierCollection();

    private void OnParticleSystemStopped()
    {
        Destroy(gameObject);
    }

    protected override void OnCollideStart(Collider collider)
    {
        if (collider.attachedRigidbody.TryGetComponent<Player>(out Player player))
        {
            modifiers.Add(player.Movement.TerminalVelocity.MultiplyBy(SourcePowerup.auxillaryTerminalVelocityMultiplier));
        }
    }

    protected override void OnCollideStop(Collider collider, bool destroyed)
    {
        if (!destroyed && collider.attachedRigidbody.TryGetComponent<Player>(out Player player))
        {
            modifiers.RevertAllFor(player.Movement.TerminalVelocity);
        }
    }
}
