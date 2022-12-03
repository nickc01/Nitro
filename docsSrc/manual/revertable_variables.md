Revertable Variables are special variables that can be easily modified and reverted back to a previous state. When added to a MonoBehaviour script, they will automatically appear in the inspector (assuming they are public or are marked with `SerializeField`)

```csharp
public class Movement : MonoBehaviour
{
	[SerializeField]
	RevertableVar<float> TerminalVelocity = 10f;

	...
}
```

![Revertable Variable Inspector](https://user-images.githubusercontent.com/12601671/118350430-381bf480-b51c-11eb-97f2-c3b1f8d40a36.PNG)

What makes Revertable Variables special compared to normal variables is because:
- Multiple different sources are able to modify the variable, and revert their modifications so the variable gets reset to its base state
- Modifications can be bound to specific objects, so when said specific object gets destroyed, the modification gets automatically reverted
- You can mark modifications with a certain priority so that they will be applied before other modifications. For example, lets say you touch a puddle, and the puddle decreases the terminal velocity by half, with a modification priority of 10. Then, at the same time, you execute a boost powerup that doubles the terminal velocity of your car, with a modification priority of 20. Because the boost powerup has a modification priority higher than the puddle, the boost powerup's modification will be applied sooner than the puddle's.

An example of where revertable variables are used can be found in the [Player](https://github.com/nickc01/Nitro/blob/master/Assets/Scripts/Player.cs) script

And examples of scripts that modify the nitro variables can be found in the [Puddle](https://github.com/nickc01/Nitro/blob/master/Assets/Scripts/Powerup%20Related/Puddle.cs) Script and the [Fire Particles](https://github.com/nickc01/Nitro/blob/master/Assets/Scripts/Powerup%20Related/FireParticles.cs) Script