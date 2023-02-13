using Assets;
using Mirror;
using System.Collections;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    private static LobbyUI _instance;
    public static LobbyUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<LobbyUI>();
            }
            return _instance;
        }
    }

    [SerializeField]
    private NetworkManager manager;

    [field: SerializeField]
    public CarCollection Selections { get; private set; }

    [SerializeField]
    private SceneTransition transitionPrefab;

    [SerializeField]
    private PlayerSlot slotPrefab;

    [field: SerializeField]
    public Transform PlayerSlotContainer { get; private set; }

    [SerializeField]
    private TextMeshProUGUI ipAddressText;
    private bool leaving = false;

    private void Start()
    {
        if (GameObject.FindObjectOfType<GameSettings>() == null)
        {
            manager.StartHost();
        }
        StartCoroutine(WaitForConnection());
    }

    private IEnumerator WaitForConnection()
    {
        yield return new WaitUntil(() => manager.mode != NetworkManagerMode.Offline);
        OnConnect();
    }

    private void OnConnect()
    {
        WebClient webClient = new WebClient();
        webClient.DownloadStringCompleted += (obj, args) =>
        {
            if (ipAddressText != null)
            {
                string result = args.Result;
                global::System.Net.IPAddress externalIP = IPAddress.Parse(result.Replace("\\r\\n", "").Replace("\\n", "").Trim());
                ipAddressText.text = $"IP Address = {externalIP}";
                ipAddressText.gameObject.SetActive(true);
            }
        };
        webClient.DownloadStringAsync(new System.Uri("http://icanhazip.com"));
    }

    [Server]
    public PlayerSlot AddLobbyUI(PlayerManager player)
    {
        PlayerSlot slot = GameObject.Instantiate(slotPrefab, PlayerSlotContainer);

        slot.Setup(player.PlayerName, player.SelectionID, player.PlayerID);

        slot.SlotID = System.Guid.NewGuid().ToString();

        if (player.isServer && !player.isLocalPlayer)
        {
            slot.KickButton.gameObject.SetActive(true);
        }

        NetworkServer.Spawn(slot.gameObject, player.gameObject);

        return slot;
    }

    [Client]
    public void UpdateReadyStatus()
    {
        bool currentStatus = PlayerManager.OwnedManager.ReadyToPlay;
        PlayerManager.OwnedManager.CMD_SetReadyStatus(!currentStatus);
    }


    public void Leave()
    {
        if (leaving)
        {
            return;
        }
        leaving = true;

        SceneTransition transition = GameObject.Instantiate(transitionPrefab);
        transition.HideInstant();
        transition.StartCoroutine(LeavingRoutine(transition));
    }

    private IEnumerator LeavingRoutine(SceneTransition transition)
    {
        yield return new WaitForSeconds(transition.Show());

        string currentScene = SceneManager.GetActiveScene().name;

        if (manager.mode == NetworkManagerMode.Host)
        {
            manager.StopHost();
        }
        else if (manager.mode == NetworkManagerMode.ClientOnly)
        {
            manager.StopClient();
        }
        else if (manager.mode == NetworkManagerMode.ServerOnly)
        {
            manager.StopServer();
        }

        yield return new WaitUntil(() => SceneManager.GetActiveScene().name != currentScene);

        yield return new WaitForSeconds(transition.HideAndDestroy());
    }
}
