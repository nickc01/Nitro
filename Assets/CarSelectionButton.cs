using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarSelectionButton : MonoBehaviour
{
    public CarCollection.Selection Selection { get; set; }

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (CarSelectionArea.Instance.CurrentSelection == Selection)
            {
                CarSelectionArea.Instance.CurrentSelection = null;
            }
            else
            {
                CarSelectionArea.Instance.CurrentSelection = Selection;
            }
        });
    }
}
