using UnityEngine;

public class Colorizer : MonoBehaviour
{
    private new Renderer renderer;
    private MaterialPropertyBlock propBlock;


    [SerializeField]
    private Color color = new Color(1, 1, 1, 1);

    public Color Color
    {
        get => color;
        set
        {
            if (color != value)
            {
                color = value;
                ColorUpdate();
            }
        }
    }


    protected virtual void OnValidate()
    {
        ColorUpdate();
    }

    private void ColorUpdate()
    {
        if (renderer == null)
        {
            renderer = GetComponentInChildren<Renderer>();
        }
        if (propBlock == null)
        {
            propBlock = new MaterialPropertyBlock();
        }

        renderer.GetPropertyBlock(propBlock);

        propBlock.SetColor("_Color", color);

        renderer.SetPropertyBlock(propBlock);
    }

    private void Awake()
    {
        ColorUpdate();
    }
}


