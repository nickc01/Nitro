Combinable Powerups are powerups that can be combined with other powerups to produce new effects. The main way Nitro is able to accomplish this is via a powerup priority system. Every [Combinable Powerup](xref:Nitro.CombinablePowerup) has a set priority which determines the order the powerups will execute.

![Electric Powerup Priority](https://user-images.githubusercontent.com/12601671/118350705-da88a780-b51d-11eb-9489-5ce25f57be25.PNG)

For example, lets say you collect a fire powerup, a water powerup, and an electric powerup, and the priorities of each of the powerups are the following:
#### Priorities: ####
- Fire Powerup : 1
- Water Powerup : 2
- Electric Powerup : 5

Since the Electric Powerup has the highest priority, that means when you go to execute the powerups, the electric powerup will execute first. Then, the electric powerup will trigger the water powerup to run, which is second in the chain. And finally, the water powerup will trigger the fire powerup to run, which is last in the chain.

If the player was to collect a fire powerup and a water powerup, the water powerup will execute first, which is to spawn a puddle at the player's location.
*Snippet from [WaterPowerup.cs](https://github.com/nickc01/Nitro/blob/master/Assets/Scripts/Powerups/WaterPowerup.cs)*
```csharp
/// <summary>
/// The main action of the combinable powerup
/// </summary>
/// <param name="previous">The previous powerup in the chain. If this is null, then the currently executing powerup is first in the chain</param>
/// <param name="position">The position of the collector the powerup is from</param>
/// <param name="rotation">The rotation of the collector the powerup is from</param>
/// <param name="runNextPowerup">A delegate used to execute the next powerup in the chain. Be sure to call this to make sure all the powerups in the chain get executed</param>
public override void Execute(CombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
{
	//If this powerup is the first in the powerup chain
	if (previous == null)
	{
		//Spawn a large puddle
		SpawnPuddle(largePuddleSize, position);
	}
	else
	{
		//Spawn a small puddle
		SpawnPuddle(smallPuddleSize, position);
	}
	//Trigger the next powerup in the powerup chain. You can tell it to execute at a certain position and rotation
	runNextPowerup(position, rotation);

	//Signal that this powerup is done with its execution
	DoneUsingPowerup();
}
```
Then, the water powerup will trigger the next powerup in the chain via runNextPowerup, which in this case is the fire powerup. This powerup will spawn a circle of fire on top of the water puddle

*Snippet from [FirePowerup.cs](https://github.com/nickc01/Nitro/blob/master/Assets/Scripts/Powerups/FirePowerup.cs)*
```csharp
/// <summary>
/// The main action of the combinable powerup
/// </summary>
/// <param name="previous">The previous powerup in the chain. If this is null, then the currently executing powerup is first in the chain</param>
/// <param name="position">The position of the collector the powerup is from</param>
/// <param name="rotation">The rotation of the collector the powerup is from</param>
/// <param name="runNextPowerup">A delegate used to execute the next powerup in the chain. Be sure to call this to make sure all the powerups in the chain get executed</param>
public override void Execute(CombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
{
	//If this is the first powerup in the chain
	if (previous == null)
	{
		//Spawn a fireball
		DoFireball(position, rotation, runNextPowerup);
	}
	//If this powerup isn't the first in the powerup chain
	else
	{
		//Spawn a circular pit of flames
		DoFirepit(position);
		//Trigger the next powerup in the chain to run
		runNextPowerup(position, rotation);
	}
	
	//Signal that this powerup is done with its execution
	DoneUsingPowerup();
}
```
The end result is a combined effect where both a puddle and fire particles are placed.

**NOTE**: If you want to be able to collect, store, and execute multiple powerups, you will need to use a [Multiple Powerup Collector](xref:Nitro.MultiplePowerupCollector) Component, rather than a [Single Powerup Collector](xref:Nitro.SinglePowerupCollector) Component

![Multiple Powerup Collector](https://user-images.githubusercontent.com/12601671/118372070-421e1180-b575-11eb-9bea-34fac147c309.PNG)

*Final Result*

![Puddle with Fire](https://user-images.githubusercontent.com/12601671/118372189-f61f9c80-b575-11eb-827f-53c412d7beef.gif)


However, if I were to swap the priorities of the water powerup and the fire powerup, an entirely new powerup combination is done. Since the Fire powerup now has a higher priority, it will be its first to execute, which is to spawn a fireball.
Then, the fire powerup will trigger the water powerup to run, which is to spawn a puddle.

The final result is a fireball that will spawn a trail of puddles as it travels in a straight line. The fireball will be destroyed either when it hits something or after a set lifetime.

![Fireball with Puddles](https://user-images.githubusercontent.com/12601671/118372578-0173c780-b578-11eb-9aa2-ab720480ed72.gif)
## Manual control

If you want more manual control over how your powerups combine with each other (like making a smoke effect when water and fire is combined), you can use the [HasPowerupInChain](xref:Nitro.CombinablePowerup.HasPowerupInChain``1) function to check if a certain powerup type is in the powerup chain. You can then use this to cause the powerup do so something different depending on what powerups are in the list.

```csharp
public override void Execute(CombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
{
    if (HasPowerupInChain<WaterPowerup>() && HasPowerupInChain<FirePowerup>())
    {
        //...Do combo that is specific to fire and water
    }
    else
    {
        //...Do general combo of the powerups
        runNextPowerup(position, rotation);
    }
}
```

Other functions to look at include:
- [GetPowerupChain](xref:Nitro.CombinablePowerup.GetPowerupChain) gets all the powerups currently in the powerup chain
- [GetPowerupIndex](xref:Nitro.CombinablePowerup.GetPowerupIndex) gets the current powerup's index in the powerup chain. This can be used in conjunction with [GetPowerupChain](xref:Nitro.CombinablePowerup.GetPowerupChain)