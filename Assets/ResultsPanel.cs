using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultsPanel : MonoBehaviour
{
    [field: SerializeField]
    public Image CarImage { get; private set; }

    [field: SerializeField]
    public TextMeshProUGUI PlayerName { get; private set; }

    [field: SerializeField]
    public TextMeshProUGUI Placement { get; private set; }
}
