using Nitro;
using System;
using UnityEngine;
using UnityEngine.Events;

public class BallPowerup : CombinablePowerup
{
    [SerializeField]
    Ball ballPrefab;

    [SerializeField]
    Vector3 force;

    public override void Execute(ICombinablePowerup previous, Vector3 position, Quaternion rotation, Action<Vector3, Quaternion> runNextPowerup)
    {
        //Spawn ball
        var ball = GameObject.Instantiate(ballPrefab,position,rotation);

        //Run the next powerup on each bounce
        ball.OnBounce += runNextPowerup;

        //Apply force
        ball.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);

        //Finish powerup after 10 seconds
        DoneUsingPowerupAfter(10f);
    }
}
