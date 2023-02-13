using Assets;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    static Countdown _instance;
    public static Countdown Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<Countdown>();
            }
            return _instance;
        }
    }

    [SerializeField]
    float countdownTime = 0.75f * 3f;

    TextMeshProUGUI textGUI;

    private void Awake()
    {
        textGUI = GetComponent<TextMeshProUGUI>();
        textGUI.enabled = false;
    }

    [Client]
    public void StartCountdownClient()
    {
        StartCoroutine(CountdownClientRoutine());
    }

    IEnumerator CountdownClientRoutine()
    {
        textGUI.text = "3";
        textGUI.enabled = true;

        yield return new WaitForSeconds(countdownTime / 3);
        textGUI.text = "2";
        yield return new WaitForSeconds(countdownTime / 3);
        textGUI.text = "1";
        yield return new WaitForSeconds(countdownTime / 3);

        textGUI.text = "Go!";

        yield return new WaitForSeconds(1.5f);
        textGUI.enabled = false;
    }

    [Server]
    public void StartCountdownServer()
    {
        StartCoroutine(CountdownServerRoutine());
    }

    IEnumerator CountdownServerRoutine()
    {
        yield return new WaitForSeconds(countdownTime);
        foreach (var player in PlayerManager.Players)
        {
            player.CarController.Settings.InputEnabled = true;
        }
    }
}
