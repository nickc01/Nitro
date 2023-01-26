using Assets;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    static LobbyUI _instance;
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
    NetworkManager manager;

    [field: SerializeField]
    public CarCollection Selections { get; private set; }

    [SerializeField]
    SceneTransition transitionPrefab;

    [SerializeField]
    PlayerSlot slotPrefab;

    [field: SerializeField]
    public Transform PlayerSlotContainer { get; private set; }

    bool leaving = false;

    private void Start()
    {
        if (GameObject.FindObjectOfType<GameSettings>() == null)
        {
            manager.StartHost();
        }
        StartCoroutine(WaitForConnection());
    }

    IEnumerator WaitForConnection()
    {
        yield return new WaitUntil(() => manager.mode != NetworkManagerMode.Offline);
        OnConnect();
    }

    void OnConnect()
    {
        
    }

    [Server]
    public PlayerSlot AddLobbyUI(PlayerManager player)
    {
        var slot = GameObject.Instantiate(slotPrefab, PlayerSlotContainer);

        slot.Setup(player.PlayerName, player.SelectionID,player.PlayerID);

        slot.SlotID = System.Guid.NewGuid().ToString();

        if (player.isServer && !player.isLocalPlayer)
        {
            slot.KickButton.gameObject.SetActive(true);
        }

        NetworkServer.Spawn(slot.gameObject, player.gameObject);

        return slot;
        //
        //slot.name = player.name;
        //slot.CarImage.sprite = Selections.Selections[player.SelectionID].Screenshot;
    }

    /*[Server]
    public void KickPlayer(uint netIDToKick, NetworkConnectionToClient sender)
    {
        //var player = sender.owned.Select(o => o.GetComponent<NewCar>()).First(c => c != null);
        //var player = sender.owned.First();
        var player = sender.identity;
        if (player.isServer)
        {
            Debug.Log("NET ID TO KICK = " + netIDToKick);
            Debug.Log("Source Player = " + player.name);
            foreach (var c in NetworkServer.connections)
            {
                Debug.Log("C Connection = " + c.Key);
                Debug.Log("C NET ID = " + c.Value.owned.First().netId);
            }
        }
    }*/

    [Client]
    public void UpdateReadyStatus()
    {
        var currentStatus = PlayerManager.OwnedManager.ReadyToPlay;
        Debug.Log("CHANGE READY STATUS TO = " + !currentStatus);

        PlayerManager.OwnedManager.CMD_SetReadyStatus(!currentStatus);
    }


    public void Leave()
    {
        if (leaving)
        {
            return;
        }
        leaving = true;

        var transition = GameObject.Instantiate(transitionPrefab);
        transition.HideInstant();
        transition.StartCoroutine(LeavingRoutine(transition));
    }

    IEnumerator LeavingRoutine(SceneTransition transition)
    {
        yield return new WaitForSeconds(transition.Show());

        var currentScene = SceneManager.GetActiveScene().name;

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
