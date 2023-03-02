The [Collidable](xref:Nitro.Collidable) and [Collidable2D](xref:Nitro.Collidable2D) components make it very easy to create hazards such as oil slicks or water puddles that can slow down drivers. When you derive from either one of these classes, you can override the [OnCollideStart](xref:Nitro.Collidable.OnCollideStart(Collider)) and [OnCollideStop](xref:Nitro.Collidable.OnCollideStop(Collider,System.Boolean)) functions to detect when the player has touched and untouched the hazard.
These components can be attached to any gameobject and they will start keeping track of collisions on the object. In order to tap into this collision data, you can override the [OnCollideStart](xref:Nitro.Collidable.OnCollideStart) and the [OnCollideStop](xref:Nitro.Collidable.OnCollideStop) events. OnCollideStart is called whenever the gameObject collides with something, while the OnCollideStop is called anytime the gameObject stops colliding with something.

You can also use a [ModifierCollection](xref:Nitro.ModifierCollection) to easily keep track of all the modifications that have been made
```csharp
using Nitro;
using UnityEngine;

public class OilSlick : MonoBehaviour
{
    //Used to easily store all the modifiers currently applied
    ModifierCollection modifiers = new ModifierCollection();

    private void Awake()
    {
        //Get the attached Collidable component and hook into the OnCollideStart and OnCollidedStop events
        var collidable = GetComponent<Collidable>();
        collidable.OnCollideStart += OnCollideStart;
        collidable.OnCollideStop += OnCollideStop;
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
```

The reason you would use the [Collidable](xref:Nitro.Collidable) class over traditional methods of detecting collision is because the Collidable class keeps track of the what it has collided with.
And because of this, it will automatically call OnCollideStop whenever the component gets disabled or destroyed, or when a collider gets destroyed while colliding with it.

This guarantees that anything done to a collider done in [OnCollideStart](xref:Nitro.Collidable.OnCollideStart) will be undone in [OnCollideStop](xref:Nitro.Collidable.OnCollideStop)

Here is the [Collidable](xref:Nitro.Collidable) class in action. An object called "Puddle" is placed in the world and has a [Puddle](https://github.com/nickc01/Nitro/blob/master/Assets/Scripts/Powerup%20Related/Puddle.cs) and [Collidable](xref:Nitro.Collidable) component attached

The puddle will cut the player's terminal velocity in half, and restore it when the puddle is no longer in contact with the player.

![Puddle V2](https://user-images.githubusercontent.com/12601671/118349803-98109c00-b518-11eb-8383-490496549dd6.gif)