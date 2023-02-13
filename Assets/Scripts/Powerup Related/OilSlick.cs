using Mirror;
using Nitro;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collidable))]
public class OilSlick : NetworkBehaviour
{
    //Used to easily store all the modifiers currently applied
    ModifierCollection modifiers = new ModifierCollection();

    private void Awake()
    {
        if (NetworkServer.active)
        {
            var collidable = GetComponent<Collidable>();
            collidable.OnCollideStart += OnCollideStart;
            collidable.OnCollideStop += OnCollideStop;
        }
    }

    //Called when this object comes in contact with another collider
    private void OnCollideStop(Collider collider, bool destroyed)
    {
        //Revert any modifiers that are bound to the collider
        modifiers.RevertAllByObject(collider);
    }

    //Called when this object is no longer in contact with another collider
    private void OnCollideStart(Collider collider)
    {
        //If the collider is a player object
        if (collider.attachedRigidbody.TryGetComponent<RollCage>(out var rc))
        {
            //Modify the player's terminal velocity, and store the modifier in the ModifierCollection
            //The modifier is also tied to the player's collider ↓↓↓↓↓↓ so it can be easily reverted later
            modifiers.Add(rc.Car.Manager.CarDrag.MultiplyBy(2f),collider);
        }
    }
}
