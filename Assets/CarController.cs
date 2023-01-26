using System.Collections;
using System.Collections.Generic;
using Assets;
using Mirror;
using Nitro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

/*public class MultiplayerPowerupCollector : MultiplePowerupCollector
{
    public override void Execute()
    {
        base.Execute();
    }
}*/


public class CarController : NetworkBehaviour
{
    public CarSettings Settings { get; private set; }

    [SyncVar]
    [HideInInspector]
    public PlayerManager Manager;

    [SerializeField]
    SceneTransition outOfBoundsTransition;

    [SerializeField]
    Transform model;

    [field: SerializeField]
    [field: FormerlySerializedAs("RollCage")]
    public RollCage RollCage { get; private set; }

    [SerializeField]
    float respawnDelay = 0.5f;

    Camera mainCamera;

    [SerializeField]
    float forwardAcceleration = 5f;

    [SerializeField]
    float rotationVelocity = 45f;

    [SerializeField]
    public float MaximumVelocity = 10f;

    [SerializeField]
    Vector3 cameraTarget;

    [SerializeField]
    float normalAdjustmentInterpolation = 3f;

    [SerializeField]
    float cameraTargetInterpolation = 5f;

    [SerializeField]
    float modelPositionInterpolation = 15f;

    [SerializeField]
    Transform frontLeftWheel;

    [SerializeField]
    Transform frontRightWheel;

    [SerializeField]
    float reverseMultiplier = 0.5f;

    [SerializeField]
    float inputInterpolationAmount = 10f;

    //[SyncVar]
    //public bool InputEnabled = false;

    //public uint GameSeed = 0;
    //public string PlayerName;
    //public int SelectionID;

    RaycastHit[] hits = new RaycastHit[1];

    Vector3 sphereLocalPos;

    float modelRotation = 0f;

    Vector3 modelLocalPos;

    [SerializeField]
    bool _controlCamera = false;

    int checkpointIndex = -1;
    Transform lastHitCheckpoint;

    MapGenerator generator;

    float previousHorizontal = 0;
    float previousVertical = 0;

    bool respawning = false;

    NetworkedMultiplePowerupCollector Collector;

    //PlayerSlot lobbySlot;

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

    private void Awake()
    {
        Settings = GetComponent<CarSettings>();
        Collector = GetComponent<NetworkedMultiplePowerupCollector>();
    }

    void Start()
    {
        //sphereLocalPos = RollCage.transform.localPosition;

        RollCage.Car = this;
        RollCage.transform.SetParent(null,true);
        sphereLocalPos = RollCage.transform.position - transform.position;
        //model.transform.SetParent(null,true);

        //modelLocalPos = transform.position - transform.TransformPoint(model.localPosition);

        transform.position = RollCage.transform.position - sphereLocalPos;

        model.transform.SetParent(null,true);
        modelLocalPos = model.transform.position - transform.position;
    }

    /*public override void OnStartLocalPlayer()
    {

    }*/

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
            //base.OnStartLocalPlayer();
            UpdateCamera();
        }

        if (!authority)
        {
            model.GetComponentInChildren<Collider>().gameObject.layer = LayerMask.NameToLayer("Car Part");
            RollCage.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        /*Debug.Log("Authority = " + authority);
        if (authority)
        {
            Debug.Log("SENDING PLAYER NAME = " + GameSettings.Instance.PlayerName);
            CMD_SendStartingStats(GameSettings.Instance.PlayerName, GameSettings.Instance.CarSelectionIndex);
        }*/
    }

    /*[Command]
    void CMD_SendStartingStats(string playerName, int selectionID)
    {
        PlayerName = playerName;
        GameSeed = GameSettings.Instance.Seed;
        SelectionID = selectionID;
        RPC_SendDataToClients(PlayerName,GameSeed, SelectionID);
        LobbyUI.Instance.AddLobbyUI(this);
    }

    [ClientRpc]
    void RPC_SendDataToClients(string playerName, uint gameSeed, int selectionID)
    {
        PlayerName = playerName;
        GameSeed = gameSeed;
        SelectionID = selectionID;
        //lobbySlot = LobbyUI.Instance.AddLobbyUI(this);
    }*/

    /*void Start()
    {
        
    }*/

    public void OnCollisionEnter(Collision collision)
    {
        if (NetworkServer.active && collision.gameObject.name == "Floor")
        {
            TriggerRespawnServer();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("TRIGGER COLLIDER = " + other.name);
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
            var index = generator.CheckPoints.IndexOf(other.transform);

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
                    //TRIGGER RESPAWN
                    //TriggerRespawnUI(respawnDelay);
                    TriggerRespawnServer();
                }
            }
        }

        Debug.Log("OTHER OBJECT = " + other.gameObject.name);

        if (other.gameObject.name == "Finish Trigger" && Manager.FinishedPosition < 0)
        {
            Settings.InputEnabled = false;
            Manager.FinishedPosition = ++MainNetworkManager.Instance.CurrentPlaceMarker;
            //TODO - TRIGGER WIN
            RPC_EnableFinishText(Manager.FinishedPosition);

            Manager.CheckAllFinishedPlayers();
        }
    }

    [TargetRpc]
    void TriggerRespawnUI(float respawnDelay, Vector3 position, Quaternion rotation)
    {
        var transition = GameObject.Instantiate(outOfBoundsTransition);
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
    void RPC_EnableFinishText(int position)
    {
        string text;

        if (position == 11 || position == 12 || position == 13)
        {
            text = $"{position}th";
        }
        if (position % 10 == 1)
        {
            text = $"{position}st";
        }
        else if (position % 10 == 2)
        {
            text = $"{position}nd";
        }
        else if (position % 10 == 3)
        {
            text = $"{position}rd";
        }
        else
        {
            text = $"{position}th";
        }

        GameCanvas.Instance.FinishText.gameObject.SetActive(true);
        GameCanvas.Instance.FinishText.text = text;
    }

    [Server]
    void TriggerRespawnServer()
    {
        if (lastHitCheckpoint == null)
        {
            lastHitCheckpoint = MapGenerator.Instance.CheckPoints[0];
        }

        Debug.Log("TRIGGER SERVER RESPAWN");
        TriggerRespawnUI(respawnDelay, lastHitCheckpoint.position,lastHitCheckpoint.rotation);

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

    Vector3 GetCamTargetPos()
    {
        //return transform.TransformPoint(
        return transform.TransformPoint(cameraTarget);
    }

    Vector3 normal = Vector3.up;

    void Update()
    {
        if (authority)
        {
            transform.position = Vector3.Lerp(transform.position, RollCage.transform.position - sphereLocalPos, modelPositionInterpolation * Time.deltaTime);
        }

        var interp = Vector3.Lerp(transform.position, RollCage.transform.position - sphereLocalPos, modelPositionInterpolation * Time.deltaTime);
        //model.transform.position = transform.position + modelLocalPos;
        model.transform.position = new Vector3(interp.x, transform.position.y + modelLocalPos.y, interp.z);
        model.transform.rotation = transform.rotation;
        //transform.rotation = model.rotation;
        if (authority && ControlCamera)
        {
            UpdateCamera();
        }

        if (Settings.InputEnabled && authority && Input.GetKeyDown(KeyCode.Space))
        {
            Collector.Execute();
        }
    }

    void FixedUpdate()
    {
        if (authority)
        {
            float vertical = 0f;
            if (Settings.InputEnabled && !respawning)
            {
                //Debug.Log("Acceleration = " + Input.GetAxis("Brake Joystick"));

                //vertical = Input.GetAxisRaw("Vertical");

                float joystickVertical = 0;
                joystickVertical += Input.GetAxisRaw("Accelerate Joystick");
                joystickVertical -= Input.GetAxisRaw("Brake Joystick");

                if (Mathf.Abs(joystickVertical) > 0.2f)
                {
                    vertical = Mathf.Lerp(vertical, joystickVertical, inputInterpolationAmount * Time.fixedDeltaTime);
                    //vertical = joystickVertical;
                }
                else
                {
                    //vertical = Mathf.Lerp(previousVertical, Input.GetAxisRaw("Vertical"), inputInterpolationAmount * Time.fixedDeltaTime);

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

            //RollCage.AddTorque(new Vector3(forwardAcceleration * Time.deltaTime,0f,0f),ForceMode.Acceleration);
            if (RollCage.OnGround) {
                RollCage.RB.AddForce(transform.forward * finalVertical * forwardAcceleration * Time.fixedDeltaTime,ForceMode.Acceleration);
            }

            //Debug.Log("Velocity = " + RollCage.RB.velocity.magnitude);

            float horizontal = 0f;

            if (Settings.InputEnabled && !respawning)
            {
                //horizontal = Input.GetAxisRaw("Horizontal");
                horizontal = Mathf.Lerp(previousHorizontal, Input.GetAxisRaw("Horizontal"), inputInterpolationAmount * Time.fixedDeltaTime);
            }

            modelRotation += rotationVelocity * Time.fixedDeltaTime * horizontal;

            //Debug.DrawRay(model.position,-transform.up * 5f,Color.black);

            //
            //Physics.SphereCastNonAlloc(transform.position, 0.1f, Vector3.down, hits, 0.1f, LayerMask.GetMask("Terrain"))

            if (Physics.RaycastNonAlloc(transform.position + new Vector3(0f, 0.1f, 0f), Vector3.down, hits, 0.125f, LayerMask.GetMask("Terrain")) > 0)
            {
                normal = Vector3.Slerp(normal,hits[0].normal,normalAdjustmentInterpolation * Time.fixedDeltaTime);
                //normal = hits[0].normal;
                Debug.DrawLine(transform.position + new Vector3(0f, 0.1f, 0f), hits[0].point, Color.black, 1f);
                Debug.Log("NORMAL = " + normal);
            }

            transform.up = normal;

            transform.rotation *= Quaternion.Euler(0f,modelRotation,0f);

            frontRightWheel.transform.localRotation = Quaternion.Euler(0f,180f + (rotationVelocity * horizontal/  3f),0f);
            frontLeftWheel.transform.localRotation = Quaternion.Euler(0f,(rotationVelocity * horizontal / 3f),0f);

            /*var velocity2D = new Vector2(RollCage.velocity.x,RollCage.velocity.z);

            if (velocity2D.magnitude > maximumVelocity)
            {
                velocity2D = velocity2D.normalized * maximumVelocity;
                RollCage.velocity = new Vector3(velocity2D.x,RollCage.velocity.y,velocity2D.y);
            }*/

            //model.transform.position = transform.position;
        }
        else
        {
            //transform.position = new Vector3();
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
