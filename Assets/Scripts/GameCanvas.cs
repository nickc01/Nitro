using Assets;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameCanvas : MonoBehaviour
{
    [Tooltip("The text component for displaying the finish message")]
    public TextMeshProUGUI FinishText;

    [SerializeField]
    [Tooltip("The results screen game object")]
    public GameObject ResultsScreen;

    [SerializeField]
    [Tooltip("The transform component of the results content")]
    public Transform ResultsContent;

    [SerializeField]
    [Tooltip("The prefab for the results panel")]
    public ResultsPanel ResultsPrefab;


    [Header("In-game Menu")]
    [SerializeField]
    GameObject MenuScreen;

    [SerializeField]
    GameObject MenuButton;

    static GameCanvas _instance;
    public static GameCanvas Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameCanvas>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        MenuScreen.SetActive(false);
    }

    public void OpenMenuButton()
    {
        MenuScreen.SetActive(true);
    }

    public void BackButton()
    {
        MenuScreen.SetActive(false);
    }

    public void QuitGameButton()
    {
        switch (GameSettings.Instance.Mode)
        {
            case PlayerMode.SinglePlayer:
                MainNetworkManager.Instance.StopHost();
                break;
            case PlayerMode.MultiPlayerHost:
                MainNetworkManager.Instance.StopServer();
                break;
            case PlayerMode.MultiplayerJoin:
                MainNetworkManager.Instance.StopClient();
                break;
            default:
                break;
        }
    }

    public void ShowMenuButton()
    {
        MenuButton.SetActive(true);
    }
}
