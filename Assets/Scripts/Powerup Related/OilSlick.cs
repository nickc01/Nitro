using Nitro;
using System.Collections.Generic;
using UnityEngine;


public class OilSlick : Collidable
{
    //Used to easily store all the modifiers currently applied
    ModifierCollection modifiers = new ModifierCollection();

    //Called when this object comes in contact with another collider
    protected override void OnCollideStart(Collider collider)
    {
        //If the collider is a player object
        if (collider.attachedRigidbody.TryGetComponent<Player>(out var player))
        {
            //Modify the player's terminal velocity, and store the modifier in the ModifierCollection
            //       The modifier is also tied to the player's collider ↓↓↓↓↓↓↓↓ so it can be easily reverted later
            modifiers.Add(player.Movement.TerminalVelocity.DivideBy(8f),collider);
        }
    }

    //Called when this object is no longer in contact with another collider
    protected override void OnCollideStop(Collider collider, bool destroyed)
    {
        //Revert any modifiers that are bound to the collider
        modifiers.RevertAllByObject(collider);
    }
}
