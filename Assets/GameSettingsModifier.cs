using UnityEngine;

namespace Assets
{


    public class GameSettingsModifier : MonoBehaviour
    {
        public void SetMode(PlayerMode mode)
        {
            GameSettings.Instance.SetMode(mode);
        }

        public void SetSinglePlayer() => SetMode(PlayerMode.SinglePlayer);
        public void SetMultiplayerHost() => SetMode(PlayerMode.MultiPlayerHost);
        public void SetMultiplayerJoin() => SetMode(PlayerMode.MultiplayerJoin);

        public void StartGame()
        {
            GameSettings.Instance.LoadLobby();
        }
    }
}
