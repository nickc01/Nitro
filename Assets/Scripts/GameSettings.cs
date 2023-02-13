using Mirror;
using Nitro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets
{
    public enum PlayerMode
    {
        SinglePlayer,
        MultiPlayerHost,
        MultiplayerJoin
    }

    public class GameSettings : MonoBehaviour
    {
        static GameSettings _instance;
        public static GameSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<GameSettings>();
                    if (_instance == null)
                    {
                        var gmPrefab = Resources.Load<GameObject>("Game Settings");
                        _instance = GameObject.Instantiate(gmPrefab, Vector3.zero, Quaternion.identity).GetComponent<GameSettings>();
                    }
                }
                return _instance;
            }
        }

        public PlayerMode Mode = PlayerMode.SinglePlayer;
        public uint Seed = 0;
        public string PlayerName = "";
        public CarCollection.Selection CarSelection;
        public int CarSelectionIndex = 0;



        [SerializeField]
        SceneTransition transitionPrefab;

        public List<CombinablePowerup> PossiblePowerups = new List<CombinablePowerup>();

        [NonSerialized]
        public List<Type> PossiblePowerupTypes;

        public bool LoadingLobby { get; private set; } = false;

        private void Awake()
        {
            PossiblePowerupTypes = PossiblePowerups.Select(p => p.GetType()).ToList();

            DontDestroyOnLoad(gameObject);
            if (_instance == null || _instance == this)
            {
                _instance = this;
            }
            else
            {
                GameObject.Destroy(gameObject);
            }
        }

        public void SetMode(PlayerMode mode)
        {
            Mode = mode;
        }

        public void LoadLobby()
        {
            if (!LoadingLobby)
            {
                LoadingLobby = true;
                StartCoroutine(LoadLobbyRoutine());
            }
        }

        public void SetCarSelection(CarCollection.Selection selection)
        {
            CarSelection = selection;
            if (selection == null)
            {
                CarSelectionIndex = -1;
            }
            else
            {
                CarSelectionIndex = CarSelectionPanel.Instance.PossibleSelections.PossibleCars.IndexOf(selection);
            }
        }

        IEnumerator LoadLobbyRoutine()
        {
            // Get the IP address from the input field
            var ipAddress = CarSelectionPanel.Instance.IPAddressField.text;

            // If no IP address, use default "127.0.0.1"
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = "127.0.0.1";
            }

            // Create a transition object
            var transition = GameObject.Instantiate(transitionPrefab);
            transition.HideInstant();
            yield return new WaitForSeconds(transition.Show());

            // Load the "City Scene"
            var sceneLoadOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("City Scene");
            yield return new WaitUntil(() => sceneLoadOp.isDone);
            yield return null;

            // Get the network manager
            var networkManager = GameObject.FindObjectOfType<MainNetworkManager>();

            // Depending on the player mode, start a single player game, join a multiplayer game, or host a multiplayer game
            if (Mode == PlayerMode.SinglePlayer)
            {
                // Set a random seed for single player mode
                GameSettings.Instance.Seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
                networkManager.maxConnections = 1;
                networkManager.StartHost();
            }
            else if (Mode == PlayerMode.MultiplayerJoin)
            {
                // Set the network address and join a multiplayer game
                networkManager.networkAddress = ipAddress;
                networkManager.StartClient();
            }
            else if (Mode == PlayerMode.MultiPlayerHost)
            {
                // Set a random seed and host a multiplayer game
                GameSettings.Instance.Seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
                networkManager.StartHost();
            }

            //Wait till ClientConnectionStatus is either "Connected" or "Disconnected"
            yield return new WaitUntil(() => networkManager.ClientConnectionStatus != MainNetworkManager.ConnectionStatus.Connecting);

            if (networkManager.ClientConnectionStatus == MainNetworkManager.ConnectionStatus.Disconnected)
            {
                ErrorDisplay.Create("Error", "Could not connect to server. The server may not be active");
                yield return new WaitForSeconds(transition.HideAndDestroy());
                LoadingLobby = false;
                yield break;
            }

            // If not single player, hide the transition
            if (Mode != PlayerMode.SinglePlayer)
            {
                yield return new WaitForSeconds(transition.HideAndDestroy());
            }

            // Set the loading lobby flag to false
            LoadingLobby = false;

            // If single player, start the game
            if (Mode == PlayerMode.SinglePlayer)
            {
                networkManager.StartGame();
            }

            // If single player, wait for all players to generate their maps and hide the transition
            if (Mode == PlayerMode.SinglePlayer)
            {
                yield return new WaitUntil(() => PlayerManager.Players.All(p => p.MapGenerated));
                yield return new WaitForSeconds(transition.HideAndDestroy());
            }
        }
    }
}
