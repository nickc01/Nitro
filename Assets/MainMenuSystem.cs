using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSystem : MonoBehaviour
{
    static MainMenuSystem _instance;
    public MainMenuSystem Instance => _instance ??= GameObject.FindObjectOfType<MainMenuSystem>();

    public List<GameObject> Cars = new List<GameObject>();

    [SerializeField]
    Panel mainMenuPanel;


    public void Quit()
    {
        Application.Quit();
    }

    private void Awake()
    {
        var panels = GetComponentsInChildren<Panel>();

        foreach (var panel in panels)
        {
            panel.gameObject.SetActive(false);
        }

        mainMenuPanel.gameObject.SetActive(true);
    }
}
