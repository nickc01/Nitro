using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupColorDisplay : MonoBehaviour
{
    public static PowerupColorDisplay Instance { get; private set; }

    [SerializeField]
    AnimationCurve InterpolationCurve;
    [SerializeField]
    Image ColorPrefab;
    [SerializeField]
    float resizeTime = 0.5f;

    RectTransform rectT;


    List<Image> addedColors = new List<Image>();

    private void Awake()
    {
        rectT = GetComponent<RectTransform>();
        Instance = this;
    }

    public static void AddColor(Color color)
    {
        Instance.StopAllCoroutines();
        var newColorImage = GameObject.Instantiate(Instance.ColorPrefab, Instance.transform);
        newColorImage.color = color;
        Instance.addedColors.Add(newColorImage);

        foreach (var addedColor in Instance.addedColors)
        {
            Instance.StartCoroutine(Instance.ResizeElement(addedColor.GetComponent<RectTransform>(), Instance.rectT.sizeDelta.x / Instance.addedColors.Count));
        }
    }

    public static void Clear()
    {
        Instance.StopAllCoroutines();
        foreach (var color in Instance.addedColors)
        {
            Destroy(color.gameObject);
        }
        Instance.addedColors.Clear();
    }



    IEnumerator ResizeElement(RectTransform obj, float newWidth)
    {
        var oldWidth = obj.sizeDelta.x;
        for (float i = 0; i < resizeTime; i += Time.deltaTime)
        {
            obj.sizeDelta = new Vector2(Mathf.Lerp(oldWidth,newWidth,InterpolationCurve.Evaluate(i / resizeTime)),obj.sizeDelta.y);
            yield return null;
        }
        obj.sizeDelta = new Vector2(newWidth,obj.sizeDelta.y);
    }
}
