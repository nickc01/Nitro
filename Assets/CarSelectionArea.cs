using Assets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CarSelectionArea : MonoBehaviour
{
    static CarSelectionArea _instance;
    public static CarSelectionArea Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<CarSelectionArea>();
            }
            return _instance;
        }
    }

    [field: SerializeField]
    public CarCollection PossibleSelections { get; private set; }

    [SerializeField]
    CarSelectionButton buttonPrefab;

    [SerializeField]
    Transform selectionArea;

    [SerializeField]
    Button startButton;

    [SerializeField]
    Image carSelection;

    [SerializeField]
    TMP_InputField nameField;

    CarCollection.Selection _currentSelection;
    public CarCollection.Selection CurrentSelection
    {
        get => _currentSelection;
        set
        {
            if (_currentSelection != value)
            {
                _currentSelection = value;
                OnSelectionUpdated?.Invoke(_currentSelection);

                carSelection.sprite = _currentSelection?.Screenshot;
                startButton.interactable = _currentSelection != null;
            }
        }
    }

    [SerializeField]
    UnityEvent<CarCollection.Selection> OnSelectionUpdated;

    private void Awake()
    {
        foreach (var selection in PossibleSelections.Selections)
        {
            var instance = GameObject.Instantiate(buttonPrefab, selectionArea);
            instance.Selection = selection;

            //instance.GetComponentInChildren<Image>().sprite = selection.Screenshot;
            instance.transform.GetChild(0).GetComponent<Image>().sprite = selection.Screenshot;
        }

        nameField.onValueChanged.AddListener(name =>
        {
            GameSettings.Instance.PlayerName = name;
        });
    }

    private void OnEnable()
    {
        nameField.text = GameSettings.Instance.PlayerName;

        nameField.gameObject.SetActive(GameSettings.Instance.Mode != PlayerMode.SinglePlayer);
    }
}
