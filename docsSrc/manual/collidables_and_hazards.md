The [Collidable](xref:Nitro.Collidable) and [Collidable2D](xref:Nitro.Collidable2D) classes make it very easy to create hazards such as oil slicks or water puddles that can slow down drivers. When you derive from either one of these classes, you can override the [OnCollideStart](xref:Nitro.Collidable.OnCollideStart(Collider)) and [OnCollideStop](xref:Nitro.Collidable.OnCollideStop(Collider,System.Boolean)) functions to detect when the player has touched and untouched the hazard.

You can also use a [ModifierCollection](xref:Nitro.ModifierCollection) to easily keep track of all the modifications that have been made
```csharp
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
        //Revert any modifiers that are tied to the collider
        modifiers.RevertAllByObject(collider);
    }
}
```

The reason you would use the [Collidable](xref:Nitro.Collidable) class over traditional methods of detecting collision is because the Collidable class keeps track of the what it has collided with.
And because of this, it will automatically call OnCollideStop whenever the component gets disabled or destroyed, or when a collider gets destroyed while colliding with it.

This guarantees that anything done to a collider done in [OnCollideStart](xref:Nitro.Collidable.OnCollideStart(Collider)) will be undone in [OnCollideStop](xref:Nitro.Collidable.OnCollideStop(Collider,System.Boolean))

Here is the [Collidable](xref:Nitro.Collidable) class in action. An object called "Puddle" is placed in the world and has a [Puddle](https://github.com/nickc01/Nitro/blob/master/Assets/Scripts/Powerup%20Related/Puddle.cs) and [Collidable](xref:Nitro.Collidable) component attached

The puddle will cut the player's terminal velocity in half, and restore it when the puddle is no longer in contact with the player.

![Puddle V2](https://user-images.githubusercontent.com/12601671/118349803-98109c00-b518-11eb-8383-490496549dd6.gif)