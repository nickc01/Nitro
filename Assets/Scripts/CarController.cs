using Assets;
using Mirror;
using Nitro;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CarController : NetworkBehaviour
{
    private Camera mainCamera; // The main camera of the game
    private RaycastHit[] hits = new RaycastHit[1]; // Array of raycast hits
    private Vector3 sphereLocalPos; // The local position of the sphere
    private float modelRotation = 0f; // The rotation of the model
    private Vector3 modelLocalPos; // The local position of the model
    private int checkpointIndex = -1; // Index of the checkpoint
    private Transform lastHitCheckpoint; // The last hit checkpoint transform
    private MapGenerator generator; // The map generator component
    private float previousHorizontal = 0; // The previous horizontal input value
    private bool respawning = false; // Flag indicating if the car is respawning

    [SerializeField]
    [Tooltip("Scene transition that occurs when the car goes out of bounds")]
    private SceneTransition outOfBoundsTransition;

    [SerializeField]
    [Tooltip("Transform that represents the model of the car")]
    private Transform model;

    [field: SerializeField]
    [field: Tooltip("The roll cage component of the car")]
    [field: FormerlySerializedAs("RollCage")]
    public RollCage RollCage { get; private set; }

    [SerializeField]
    [Tooltip("The delay before the car respawns")]
    private float respawnDelay = 0.5f;

    [SerializeField]
    [Tooltip("The forward acceleration of the car")]
    private float forwardAcceleration = 5f;

    [SerializeField]
    [Tooltip("The rotation velocity of the car")]
    private float rotationVelocity = 45f;

    [SerializeField]
    [Tooltip("The maximum velocity of the car")]
    public float MaximumVelocity = 10f;

    [SerializeField]
    [Tooltip("The target position for the camera")]
    private Vector3 cameraTarget;

    [SerializeField]
    [Tooltip("The interpolation value for adjusting the camera normal")]
    private float normalAdjustmentInterpolation = 3f;

    [SerializeField]
    [Tooltip("The interpolation value for adjusting the camera target")]
    private float cameraTargetInterpolation = 5f;

    [SerializeField]
    [Tooltip("The interpolation value for adjusting the model position")]
    private float modelPositionInterpolation = 15f;

    [SerializeField]
    [Tooltip("The front left wheel transform")]
    private Transform frontLeftWheel;

    [SerializeField]
    [Tooltip("The front right wheel transform")]
    private Transform frontRightWheel;

    [SerializeField]
    [Tooltip("The multiplier for reverse movement")]
    private float reverseMultiplier = 0.5f;

    [SerializeField]
    [Tooltip("The interpolation amount for input")]
    private float inputInterpolationAmount = 10f;

    [Tooltip("The settings for the car")]
    public CarSettings Settings { get; private set; }

    [SyncVar]
    [HideInInspector]
    [Tooltip("The manager of the player")]
    public PlayerManager Manager;

    [SerializeField]
    [Tooltip("Controls whether the camera follows the car")]
    private bool _controlCamera = false;

    [Tooltip("The powerup collector for the car")]
    private NetworkedMultiplePowerupCollector Collector;

    [SerializeField]
    Vector2 pitchMinMax = new Vector2(0f,0.5f);

    public bool ControlCamera
    {
        get => _controlCamera;
        set
        {
            if (_controlCamera != value)
            {
                _controlCamera = value;
                if (_controlCamera == true)
                {
                    UpdateCamera();
                }
            }
        }
    }

    AudioSource audioSource;

    private void Awake()
    {
        Settings = GetComponent<CarSettings>();
        Collector = GetComponent<NetworkedMultiplePowerupCollector>();
        audioSource = GetComponent<AudioSource>();

        foreach (var light in GetComponentsInChildren<Light>())
        {
            GameObject.Destroy(light.gameObject);
        }

        foreach (var camera in GetComponentsInChildren<Camera>())
        {
            GameObject.Destroy(camera.gameObject);
        }
    }

    private void Start()
    {
        RollCage.Car = this;
        RollCage.transform.SetParent(null, true);
        sphereLocalPos = RollCage.transform.position - transform.position;
        transform.position = RollCage.transform.position - sphereLocalPos;

        model.transform.SetParent(null, true);
        modelLocalPos = model.transform.position - transform.position;
    }

    private void UpdateCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindObjectOfType<Camera>();

            mainCamera.transform.position = GetCamTargetPos();
            mainCamera.transform.rotation = model.transform.rotation;
        }

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, GetCamTargetPos(), cameraTargetInterpolation * Time.deltaTime);
        mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, model.transform.rotation, cameraTargetInterpolation * Time.deltaTime);
    }

    public override void OnStartClient()
    {
        if (authority && ControlCamera)
        {
            UpdateCamera();
        }

        if (!authority)
        {
            model.GetComponentInChildren<Collider>().gameObject.layer = LayerMask.NameToLayer("Car Part");
            RollCage.gameObject.layer = LayerMask.NameToLayer("Obstacle");
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (NetworkServer.active && collision.gameObject.name == "Floor")
        {
            TriggerRespawnServer();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!NetworkServer.active)
        {
            return;
        }
        if (generator == null)
        {
            generator = MapGenerator.Instance;
        }

        if (other.name == "Checkpoint")
        {
            int index = generator.CheckPoints.IndexOf(other.transform);

            if (checkpointIndex == -1)
            {
                checkpointIndex = index;
                lastHitCheckpoint = other.transform;
                return;
            }

            if (index > 0)
            {
                if (index <= checkpointIndex + 3 && index > checkpointIndex)
                {
                    checkpointIndex = index;
                    lastHitCheckpoint = other.transform;
                }
                else if (index > checkpointIndex + 3)
                {
                    TriggerRespawnServer();
                }
            }
        }

        if (other.gameObject.name == "Finish Trigger" && Manager.FinishedPosition < 0)
        {
            Settings.InputEnabled = false;
            Manager.FinishedPosition = ++MainNetworkManager.Instance.CurrentPlaceMarker;
            RPC_EnableFinishText(Manager.FinishedPosition);

            Manager.CheckAllFinishedPlayers();
        }
    }

    [TargetRpc]
    private void TriggerRespawnUI(float respawnDelay, Vector3 position, Quaternion rotation)
    {
        SceneTransition transition = GameObject.Instantiate(outOfBoundsTransition);
        transition.HideInstant();
        transition.Show();

        StartCoroutine(Routine(transition));

        IEnumerator Routine(SceneTransition transition)
        {
            respawning = true;
            yield return new WaitForSeconds(transition.TransitionTime);
            RollCage.transform.position = position + new Vector3(0f, 0.1f, 0f);
            RollCage.RB.velocity = default;
            RollCage.RB.angularVelocity = default;
            transform.rotation = rotation;
            yield return new WaitForSeconds(respawnDelay - transition.TransitionTime);
            transition.HideAndDestroy();
            respawning = false;
        }
    }

    [TargetRpc]
    private void RPC_EnableFinishText(int position)
    {
        GameCanvas.Instance.FinishText.gameObject.SetActive(true);
        GameCanvas.Instance.FinishText.text = Utilities.CalculatePlacement(position);
    }

    [Server]
    private void TriggerRespawnServer()
    {
        if (lastHitCheckpoint == null)
        {
            lastHitCheckpoint = MapGenerator.Instance.CheckPoints[0];
        }

        TriggerRespawnUI(respawnDelay, lastHitCheckpoint.position, lastHitCheckpoint.rotation);

        StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(outOfBoundsTransition.TransitionTime);
            yield return new WaitForSeconds(respawnDelay / 2f);
            yield return new WaitForSeconds(respawnDelay / 2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(GetCamTargetPos(), new Vector3(0.1f, 0.1f, 0.1f));
    }

    private Vector3 GetCamTargetPos()
    {
        return transform.TransformPoint(cameraTarget);
    }

    private Vector3 normal = Vector3.up;

    private void Update()
    {
        if (authority)
        {
            transform.position = Vector3.Lerp(transform.position, RollCage.transform.position - sphereLocalPos, modelPositionInterpolation * Time.deltaTime);
        }

        Vector3 interp = Vector3.Lerp(transform.position, RollCage.transform.position - sphereLocalPos, modelPositionInterpolation * Time.deltaTime);
        model.transform.position = new Vector3(interp.x, transform.position.y + modelLocalPos.y, interp.z);
        model.transform.rotation = transform.rotation;
        if (authority && ControlCamera)
        {
            UpdateCamera();
        }

        if (Settings.InputEnabled && authority && Input.GetKeyDown(KeyCode.Space))
        {
            Collector.Execute();
        }

        var velocity = RollCage.RB.velocity;
        audioSource.pitch = Mathf.LerpUnclamped(pitchMinMax.x, pitchMinMax.y,new Vector2(velocity.x, velocity.z).magnitude);
    }

    private void FixedUpdate()
    {
        if (authority)
        {
            float vertical = 0f;
            if (Settings.InputEnabled && !respawning)
            {
                float joystickVertical = 0;
                joystickVertical += Input.GetAxisRaw("Accelerate Joystick");
                joystickVertical -= Input.GetAxisRaw("Brake Joystick");

                if (Mathf.Abs(joystickVertical) > 0.2f)
                {
                    vertical = Mathf.Lerp(vertical, joystickVertical, inputInterpolationAmount * Time.fixedDeltaTime);
                }
                else
                {
                    float newVertical = 0f;

                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                    {
                        newVertical += 1f;
                    }
                    if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                    {
                        newVertical -= 1f;
                    }

                    vertical = Mathf.Lerp(vertical, newVertical, inputInterpolationAmount * Time.fixedDeltaTime);
                }
            }

            float finalVertical = vertical;

            if (vertical < 0f)
            {
                finalVertical *= reverseMultiplier;
            }

            if (RollCage.OnGround)
            {
                RollCage.RB.AddForce(transform.forward * finalVertical * forwardAcceleration * Time.fixedDeltaTime, ForceMode.Acceleration);
            }

            float horizontal = 0f;

            if (Settings.InputEnabled && !respawning)
            {
                horizontal = Mathf.Lerp(previousHorizontal, Input.GetAxisRaw("Horizontal"), inputInterpolationAmount * Time.fixedDeltaTime);
            }

            modelRotation += rotationVelocity * Time.fixedDeltaTime * horizontal;

            if (Physics.RaycastNonAlloc(transform.position + new Vector3(0f, 0.1f, 0f), Vector3.down, hits, 0.125f, LayerMask.GetMask("Terrain")) > 0)
            {
                normal = Vector3.Slerp(normal, hits[0].normal, normalAdjustmentInterpolation * Time.fixedDeltaTime);
                Debug.DrawLine(transform.position + new Vector3(0f, 0.1f, 0f), hits[0].point, Color.black, 1f);
            }

            transform.up = normal;

            transform.rotation *= Quaternion.Euler(0f, modelRotation, 0f);

            if (frontRightWheel != null)
            {
                frontRightWheel.transform.localRotation = Quaternion.Euler(0f, 180f + (rotationVelocity * horizontal / 3f), 0f);
            }
            if (frontLeftWheel != null)
            {
                frontLeftWheel.transform.localRotation = Quaternion.Euler(0f, (rotationVelocity * horizontal / 3f), 0f);
            }
        }
    }

    [TargetRpc]
    public void AddForce(Vector3 force, ForceMode mode)
    {
        RollCage.RB.AddForce(force, mode);
    }

    private void OnDestroy()
    {
        if (model != null)
        {
            GameObject.Destroy(model.gameObject);
        }

        if (RollCage != null)
        {
            GameObject.Destroy(RollCage.gameObject);
        }
    }
}
