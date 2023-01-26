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
                        //var gm = new GameObject("Game Settings");
                        //_instance = gm.AddComponent<GameSettings>();
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

        //public NetworkRoomManager lobby;

        [SerializeField]
        SceneTransition transitionPrefab;

        public List<CombinablePowerup> PossiblePowerups = new List<CombinablePowerup>();

        [NonSerialized]
        public List<Type> PossiblePowerupTypes;

        public bool LoadingLobby { get; private set; } = false;

        private void Awake()
        {
            PossiblePowerupTypes = PossiblePowerups.Select(p => p.GetType()).ToList();

            foreach (var type in PossiblePowerupTypes)
            {
                Debug.Log("Possible Powerup Type = " + type);
            }

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
                CarSelectionIndex = CarSelectionArea.Instance.PossibleSelections.Selections.IndexOf(selection);
            }
        }

        IEnumerator LoadLobbyRoutine()
        {
            var transition = GameObject.Instantiate(transitionPrefab);
            transition.HideInstant();
            yield return new WaitForSeconds(transition.Show());

            var sceneLoadOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("City Scene");
            yield return new WaitUntil(() => sceneLoadOp.isDone);
            yield return null;

            var networkManager = GameObject.FindObjectOfType<MainNetworkManager>();

            if (Mode == PlayerMode.SinglePlayer)
            {
                GameSettings.Instance.Seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
                networkManager.maxConnections = 1;
                networkManager.StartHost();
            }
            else if (Mode == PlayerMode.MultiplayerJoin)
            {
                networkManager.StartClient();
            }
            else if (Mode == PlayerMode.MultiPlayerHost)
            {
                GameSettings.Instance.Seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
                networkManager.StartHost();
            }

            yield return new WaitUntil(() => networkManager.mode != NetworkManagerMode.Offline);

            if (Mode != PlayerMode.SinglePlayer)
            {
                yield return new WaitForSeconds(transition.HideAndDestroy());
            }

            LoadingLobby = false;

            if (Mode == PlayerMode.SinglePlayer)
            {
                networkManager.StartGame();
            }

            if (Mode == PlayerMode.SinglePlayer)
            {
                yield return new WaitUntil(() => PlayerManager.Players.All(p => p.MapGenerated));
                yield return new WaitForSeconds(transition.HideAndDestroy());
            }
        }
    }
}
