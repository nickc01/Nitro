﻿using Mirror;
using Nitro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class PlayerManager : NetworkBehaviour
    {
        static HashSet<PlayerManager> players = new HashSet<PlayerManager>();
        public static IEnumerable<PlayerManager> Players => players;

        public static PlayerManager OwnedManager { get; private set; }

        [SyncVar]
        public CarController CarController;

        [SyncVar]
        public CarSettings CarSettings;


        [SyncVar]
        [HideInInspector]
        public uint GameSeed = 0;

        [SyncVar]
        public string PlayerName;

        [SyncVar]
        public int SelectionID;

        [SerializeField]
        SceneTransition transitionPrefab;

        [SyncVar]
        [NonSerialized]
        public bool MapGenerated = false;

        [SyncVar(hook = nameof(UpdateReadyStatus))]
        [NonSerialized]
        public bool ReadyToPlay = false;

        [SyncVar]
        public string PlayerID;

        [SyncVar]
        string slotID;

        bool everyoneGeneratedTheirMaps = false;

        [SyncVar]
        public int FinishedPosition = -1;

        [SerializeField]
        public RevertableVar<float> CarDrag = 2;

        [SerializeField]
        public RevertableVar<float> CarMaxVelocity = 100f;

        [NonSerialized]
        PlayerSlot server_slot;

        public override void OnStartServer()
        {
            players.Add(this);
            base.OnStartServer();
            SelectionID = GameSettings.Instance.CarSelectionIndex;
            server_slot = LobbyUI.Instance.AddLobbyUI(this);
            slotID = server_slot.SlotID;

            CarDrag.OnValueUpdated += OnDragUpdated;
            CarMaxVelocity.OnValueUpdated += OnMaxVelocityUpdated;
        }

        void OnDragUpdated(float oldValue, float newValue)
        {
            CarSettings.DragAmount = newValue;
        }

        void OnMaxVelocityUpdated(float oldValue, float newValue)
        {
            CarSettings.MaximumVelocity = newValue;
        }

        public override void OnStartClient()
        {
            players.Add(this);
            base.OnStartClient();


            if (isOwned)
            {
                OwnedManager = this;
                CMD_SendInfoToServer(GameSettings.Instance.PlayerName, GameSettings.Instance.CarSelectionIndex);
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            players.Remove(this);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            players.Remove(this);
        }

        [Command]
        void CMD_SendInfoToServer(string playerName, int selectionID)
        {
            GameSeed = GameSettings.Instance.Seed;
            PlayerName = playerName;
            SelectionID = selectionID;
            PlayerID = Guid.NewGuid().ToString();

            server_slot.SelectionID = SelectionID;
            server_slot.Name = PlayerName;
            server_slot.PlayerID = PlayerID;

            server_slot.CarImage.sprite = LobbyUI.Instance.Selections.PossibleCars[selectionID].Screenshot;
            server_slot.NameText.text = PlayerName;
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
        }


        [TargetRpc]
        public void TargetStartGameClient()
        {
            MainNetworkManager.Instance.GameStarted = true;
            StartCoroutine(StartGameClientRoutine());
        }

        IEnumerator StartGameClientRoutine()
        {
            var transition = GameObject.Instantiate(transitionPrefab);
            transition.HideInstant();
            yield return new WaitForSeconds(transition.Show());
            LobbyUI.Instance.gameObject.SetActive(false);
            MapGenerator.Instance.GenerateMap(GameSeed);
            CMD_DoneGeneratingMap();
            yield return new WaitUntil(() => everyoneGeneratedTheirMaps);
            yield return new WaitForSeconds(transition.HideAndDestroy());
        }

        [Command]
        void CMD_DoneGeneratingMap()
        {
            MapGenerated = true;
        }

        [ClientRpc]
        public void RPC_EveryonesReady()
        {
            everyoneGeneratedTheirMaps = true;
        }


        [ClientRpc]
        public void RpcGenerateMap()
        {
            if (isServer)
            {
                return;
            }
            MapGenerator.Instance.GenerateMap(GameSeed);
            CMD_FinishedGeneratingMap();
        }

        [Command]
        [Server]
        void CMD_FinishedGeneratingMap()
        {
            MapGenerated = true;
        }

        [Command]
        public void CMD_SetReadyStatus(bool status)
        {
            ReadyToPlay = status;
            bool ready = true;
            foreach (var player in Players)
            {
                if (!player.ReadyToPlay)
                {
                    ready = false;
                    break;
                }
            }

            if (ready)
            {
                MainNetworkManager.Instance.StartGame();
            }
        }


        [Command]
        public void StartCountdownServer()
        {
            Countdown.Instance.StartCountdownServer();
        }

        [TargetRpc]
        public void StartCountdownClient()
        {
            Countdown.Instance.StartCountdownClient();
        }


        [Server]
        public void CheckAllFinishedPlayers()
        {
            if (Players.All(p => p.FinishedPosition > 0))
            {
                DisplayResults();
            }
        }

        [ClientRpc]
        void DisplayResults()
        {
            GameCanvas.Instance.FinishText.gameObject.SetActive(false);
            GameCanvas.Instance.ResultsScreen.SetActive(true);
            foreach (var player in Players.OrderBy(p => p.FinishedPosition))
            {
                var instance = GameObject.Instantiate(GameCanvas.Instance.ResultsPrefab,GameCanvas.Instance.ResultsContent);

                if (GameSettings.Instance.CarSelection == null)
                {
                    instance.CarImage.sprite = CarCollection.Instance.PossibleCars[0].Screenshot;
                }
                else
                {
                    instance.CarImage.sprite = GameSettings.Instance.CarSelection.Screenshot;
                }
                instance.PlayerName.text = player.PlayerName;
                instance.Placement.text = Utilities.CalculatePlacement(player.FinishedPosition);
            }
        }

        void UpdateReadyStatus(bool oldReadyStatus, bool newReadyStatus)
        {
            foreach (var slot in GameObject.FindObjectsOfType<PlayerSlot>())
            {
                if (PlayerID == slot.PlayerID)
                {
                    slot.ReadyIndicator.SetActive(newReadyStatus);
                    break;
                }
            }
        }
    }
}