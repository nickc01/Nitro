using Nitro;
using UnityEngine;

public class OilSlick : Collidable
{
    ModifierCollection modifiers = new ModifierCollection();

    protected override void OnCollideStart(Collider collider)
    {
        if (collider.attachedRigidbody.TryGetComponent<Player>(out Player player))
        {
            using (player.Movement.TerminalVelocity.DivideBy(8f))
            {

            }
            modifiers.Add(player.Movement.TerminalVelocity.DivideBy(8f));
        }
    }

    protected override void OnCollideStop(Collider collider, bool destroyed)
    {
        if (collider.attachedRigidbody.TryGetComponent<Player>(out Player player))
        {
            modifiers.RevertAllFor(player.Movement.TerminalVelocity);
        }
    }
}
