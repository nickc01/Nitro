using Assets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CarSelectionPanel : MonoBehaviour
{
    static CarSelectionPanel _instance; // Singleton instance of the car selection area

    public static CarSelectionPanel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<CarSelectionPanel>();
            }
            return _instance;
        }
    }

    [field: SerializeField]
    [Tooltip("The possible selections of cars")]
    public CarCollection PossibleSelections { get; private set; }

    [SerializeField]
    [Tooltip("The button prefab for car selection")]
    CarSelectionButton buttonPrefab;

    [SerializeField]
    [Tooltip("The transform of the car selection area")]
    Transform selectionArea;

    [SerializeField]
    [Tooltip("The start button for the game")]
    Button startButton;

    [SerializeField]
    [Tooltip("The image for car selection")]
    Image carSelection;

    [SerializeField]
    [Tooltip("The input field for the player's name")]
    TMP_InputField nameField;

    [Tooltip("The input field for the server's IP address")]
    public TMP_InputField IPAddressField;


    //Declare variable to store current car selection
    CarCollection.Selection _currentSelection;

    public CarCollection.Selection CurrentSelection
    {
        get => _currentSelection;
        //Set the current car selection and update UI elements
        set
        {
            //Check if the value being set is different from the current value
            if (_currentSelection != value)
            {
                //Set the current car selection
                _currentSelection = value;

                //Invoke the OnSelectionUpdated event and pass the current selection as argument
                OnSelectionUpdated?.Invoke(_currentSelection);

                //Update the car selection sprite
                carSelection.sprite = _currentSelection?.Screenshot;

                //Set the start button interactable status based on if there is a current selection
                startButton.interactable = _currentSelection != null;
            }
        }
    }

    [SerializeField]
    UnityEvent<CarCollection.Selection> OnSelectionUpdated;

    //Initialize the car selection area in the Awake method
    private void Awake()
    {
        //Instantiate buttons for each possible car selection
        foreach (var selection in PossibleSelections.PossibleCars)
        {
            //Instantiate a button and set its properties
            var instance = GameObject.Instantiate(buttonPrefab, selectionArea);
            instance.Selection = selection;
            instance.transform.GetChild(0).GetComponent<Image>().sprite = selection.Screenshot;
        }

        //Add a listener to the name field to update the player name in the game settings
        nameField.onValueChanged.AddListener(name =>
        {
            GameSettings.Instance.PlayerName = name;
        });
    }

    //Enable the player name field in the OnEnable method
    private void OnEnable()
    {
        //Set the text of the name field to the player name in the game settings
        nameField.text = GameSettings.Instance.PlayerName;

        //Only show the name field if the game is in multiplayer mode
        nameField.gameObject.SetActive(GameSettings.Instance.Mode != PlayerMode.SinglePlayer);

        //Only show the ip field if the player is joining a lobby
        IPAddressField.gameObject.SetActive(GameSettings.Instance.Mode == PlayerMode.MultiplayerJoin);
    }
}
