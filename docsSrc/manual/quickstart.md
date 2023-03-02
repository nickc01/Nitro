Here is a quick start guide with setting up a simple powerup system

**- Step 1 : Create a powerup**

The first thing you want to do is create a powerup for you game, whether it be a boost powerup for making your car go faster, or an attack powerup for making your soldier attack twice as fast. For this example, we will just create a simple powerup that displays a message to the console:

```csharp
using Nitro;
using UnityEngine;

public class SimplePowerup : Powerup
{
	public override void DoAction()
	{
		Debug.Log("Powerup Executed!");
	}
}
```
With this script now created, create a new object in the hierarchy and add the SimplePowerup component to the object. There should also be a collider object, like a box collider, to allow other objects to pick it up.

![simple-powerup-object](https://user-images.githubusercontent.com/12601671/118348759-0140e100-b512-11eb-8f74-3f54f44a9b81.PNG)

**- Step 2 : Create a collector**

Now we need a [Collector](xref:Nitro.Collector) that will collect the powerup. To do that, add a new object called "Player" and that object will contain a component called [SinglePowerupCollector](xref:Nitro.SinglePowerupCollector). Make sure the player also has a collider and rigidbody attached so that it can automatically collect the powerup when it collides with it.

![Collector Component](https://user-images.githubusercontent.com/12601671/205415242-47b3dd2b-3b01-4de7-aaa1-4cd42e7e2d02.PNG)

How the player moves around to collect the powerup is up to you. In this example, a script called "[CarMovement](https://github.com/nickc01/Nitro/blob/master/Assets/Scripts/CarMovement.cs)" is used to move the player around using the arrow keys

**- Step 3 : A way of executing the powerup**

The last thing that is needed is a way of executing the powerup. For this example, we will create a new script so that when the "P" key is pressed, the powerup will be executed:
```csharp
using Nitro;
using UnityEngine;

public class PowerupExecuter : MonoBehaviour
{
	SinglePowerupCollector collector;

	private void Awake()
	{
		collector = GetComponent<SinglePowerupCollector>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			collector.Execute();
		}
	}
}
```

Now you can add this script to the player and you are good to go. When the player collides with the powerup, the [SinglePowerupCollector](xref:Nitro.SinglePowerupCollector) component on the player will automatically pick up the powerup. Then, when you press the "P" key, the powerup is then executed

![Powerup Collecting](https://user-images.githubusercontent.com/12601671/118349033-c344bc80-b513-11eb-84db-abd876eac4f5.gif)