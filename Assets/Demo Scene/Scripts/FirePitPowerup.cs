using Nitro;
using System;
using UnityEngine;

public class FirePitPowerup : CombinablePowerup
{
    [SerializeField]
    GameObject firePrefab;

    public override void Execute(ICombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
    {
        //Create the fire pit
        GameObject.Instantiate(firePrefab, position, rotation);

        //Run next powerup in chain
        runNextPowerup(position, rotation);

        //Powerup has finished
        DoneUsingPowerup();
    }
}
