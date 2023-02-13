using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorDisplay : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI titleText;

    [SerializeField]
    TextMeshProUGUI messageText;

    [SerializeField]
    float clearTime = 0.25f;

    public string TitleText
    {
        get { return titleText.text; }
        set { titleText.text = value; }
    }

    public string MessageText
    {
        get { return messageText.text; }
        set { messageText.text = value; }
    }

    public static ErrorDisplay Create(string titleText, string messageText)
    {
        var prefab = Resources.Load<GameObject>("Error Display");
        var instance = GameObject.Instantiate(prefab).GetComponent<ErrorDisplay>();
        GameObject.DontDestroyOnLoad(instance);

        instance.TitleText = titleText;
        instance.MessageText = messageText;

        return instance;
    }

    public void Clear()
    {
        Graphic[] graphics = GetComponentsInChildren<Graphic>();
        foreach (Graphic graphic in graphics)
        {
            StartCoroutine(FadeOut(graphic));
        }
        Destroy(gameObject,clearTime);
    }

    private IEnumerator FadeOut(Graphic graphic)
    {
        float t = 0;
        Color originalColor = graphic.color;
        while (t < 1)
        {
            t += Time.deltaTime / clearTime;
            graphic.color = Color.Lerp(originalColor, new Color(originalColor.r, originalColor.g, originalColor.b, 0), t);
            yield return null;
        }
    }
}
