using Nitro;
using UnityEngine;

public class Puddle : Collidable
{
    private ModifierCollection modifiers = new ModifierCollection();

    [SerializeField]
    private float terminalVelocityMultiplier = 0.5f;

    [SerializeField]
    private Vector3 spawnOffset;

    [SerializeField]
    private float puddleLifeTime = 5f;

    private void Awake()
    {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * 100f), Color.red, 10f);
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit info, 100f))
        {
            transform.position = info.point + spawnOffset;
        }

        Destroy(gameObject, puddleLifeTime);
    }

    protected override void OnCollideStart(Collider collider)
    {
        if (collider.attachedRigidbody.TryGetComponent<Player>(out Player player))
        {
            modifiers.Add(player.Movement.TerminalVelocity.MultiplyBy(terminalVelocityMultiplier));
        }
    }

    protected override void OnCollideStop(Collider collider, bool destroyed)
    {
        if (!destroyed && collider.attachedRigidbody.TryGetComponent<Player>(out Player player))
        {
            modifiers.RevertAllFor(player.Movement.TerminalVelocity);
        }
    }
}
