using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PanelSwitcher : MonoBehaviour
{
    [SerializeField]
    Panel destPanel;

    [SerializeField]
    float time = 0.5f;

    [SerializeField]
    Panel.SlideDirection slideDirection;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (destPanel != null)
            {
                var sourcePanel = GetComponentInParent<Panel>();
                if (sourcePanel != null)
                {
                    sourcePanel.SwitchToPanelWithTime(destPanel, slideDirection, time);
                }
            }
        });
    }
}
