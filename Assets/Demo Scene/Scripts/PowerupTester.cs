using Nitro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupTester : MonoBehaviour
{
    [SerializeField]
    List<Powerup> powerups;

    private void Awake()
    {
        var collector = GetComponent<ICollector>();
        foreach (var powerup in powerups)
        {
            collector.CollectPowerup(powerup);
        }
        collector.Execute();
    }
}
