using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarSelectionButton : MonoBehaviour
{
    //Public property to get and set the car selection associated with the button
    public CarCollection.Selection Selection { get; set; }

    //Add a listener to the button click event in the Awake method
    private void Awake()
    {
        //Get the button component and add a listener to the onClick event
        GetComponent<Button>().onClick.AddListener(() =>
        {
            //Check if the button is the current selection
            if (CarSelectionPanel.Instance.CurrentSelection == Selection)
            {
                //Clear the current selection and related game settings
                CarSelectionPanel.Instance.CurrentSelection = null;
                GameSettings.Instance.CarSelection = null;
                GameSettings.Instance.CarSelectionIndex = 0;
            }
            else
            {
                //Set the current selection and update related game settings
                CarSelectionPanel.Instance.CurrentSelection = Selection;
                GameSettings.Instance.CarSelection = Selection;
                GameSettings.Instance.CarSelectionIndex = CarCollection.Instance.PossibleCars.IndexOf(Selection);
            }
        });
    }
}
