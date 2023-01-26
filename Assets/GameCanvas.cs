using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameCanvas : MonoBehaviour
{
    public TextMeshProUGUI FinishText;

    public GameObject ResultsScreen;

    public Transform ResultsContent;

    public ResultsPanel ResultsPrefab;


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
}
