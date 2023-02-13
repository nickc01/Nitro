using UnityEngine;

namespace Assets
{
    // This class is a component for modifying the game settings
    public class GameSettingsModifier : MonoBehaviour
    {
        // Sets the player mode in the game settings instance
        public void SetMode(PlayerMode mode)
        {
            GameSettings.Instance.SetMode(mode);
        }

        // Sets the player mode to single player in the game settings instance
        public void SetSinglePlayer() => SetMode(PlayerMode.SinglePlayer);

        // Sets the player mode to multiplayer host in the game settings instance
        public void SetMultiplayerHost() => SetMode(PlayerMode.MultiPlayerHost);

        // Sets the player mode to multiplayer join in the game settings instance
        public void SetMultiplayerJoin() => SetMode(PlayerMode.MultiplayerJoin);

        // Loads the lobby in the game settings instance
        public void StartGame()
        {
            GameSettings.Instance.LoadLobby();
        }
    }

}
