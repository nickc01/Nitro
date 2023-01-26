using Mirror;

public class CarSettings : NetworkBehaviour
{
    [SyncVar]
    public bool InputEnabled = false;

    [SyncVar(hook = nameof(UpdateDragAmount))]
    public float DragAmount = 2f;

    [SyncVar(hook = nameof(UpdateMaximumVelocity))]
    public float MaximumVelocity = 100f;

    CarController controller;

    void Awake()
    {
        controller = GetComponent<CarController>();
    }

    void UpdateDragAmount(float oldValue, float newValue)
    {
        controller.RollCage.RB.drag = newValue;
    }

    void UpdateMaximumVelocity(float oldValue, float newValue)
    {
        controller.MaximumVelocity = newValue;
    }
}
