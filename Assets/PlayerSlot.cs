using Assets;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : NetworkBehaviour
{
    public static Dictionary<string, PlayerSlot> SlotsByID = new Dictionary<string, PlayerSlot>();

    public static PlayerSlot OwnedSlot { get; private set; }


    [field: SerializeField]
    public Image CarImage { get; private set; }

    [field: SerializeField]
    public TextMeshProUGUI NameText { get; private set; }

    [field: SerializeField]
    public Button KickButton { get; private set; }

    [field: SerializeField]
    public GameObject ReadyIndicator { get; private set; }




    [SyncVar(hook = nameof(UpdateSelectionID))]
    public int SelectionID;

    [SyncVar(hook = nameof(UpdateName))]
    public string Name;

    [SyncVar]
    public string PlayerID;

    [SyncVar]
    public string SlotID;


    private void Awake()
    {
        transform.SetParent(LobbyUI.Instance.PlayerSlotContainer);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        SlotsByID.TryAdd(SlotID, this);
        if (isOwned)
        {
            OwnedSlot = this;
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        SlotsByID.TryAdd(SlotID, this);
        KickButton.onClick.AddListener(() =>
        {
            foreach (var player in PlayerManager.Players)
            {
                Debug.Log("Player = " + player);
                player.ReadyToPlay = false;
            }
            netIdentity.connectionToClient.Disconnect();
        });
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
    }

    /*void KickPlayer(uint netIDToKick, NetworkConnectionToClient sender = null)
    {
        LobbyUI.Instance.KickPlayer(netIDToKick, sender);
    }*/




    void UpdateSelectionID(int oldSelectionID, int newSelectionID)
    {
        CarImage.sprite = LobbyUI.Instance.Selections.Selections[newSelectionID].Screenshot;
    }

    void UpdateName(string oldName, string newName)
    {
        NameText.text = newName;
    }

    [Server]
    public void Setup(string name, int selectionID, string playerID)
    {
        Name = name;
        SelectionID = selectionID;
        PlayerID = playerID;

        UpdateSelectionID(0, SelectionID);
        UpdateName("", Name);
    }

    public override void OnStopServer()
    {
        SlotsByID.Remove(SlotID);
        base.OnStopServer();
    }

    public override void OnStopClient()
    {
        SlotsByID.Remove(SlotID);
        base.OnStopClient();
    }

    /*public override void OnStartServer()
    {
        var ownedPlayer = this.connectionToClient.owned.Select(o => o.GetComponent<NewCar>()).First(c => c != null);

        SelectionID = ownedPlayer.SelectionID;
        Name = ownedPlayer.PlayerName;

        UpdateSelectionID(0, SelectionID);
        UpdateName("",Name);

        base.OnStartServer();
    }*/
}
