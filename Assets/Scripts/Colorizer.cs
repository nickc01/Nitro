using UnityEngine;

public class Colorizer : MonoBehaviour
{
    private new Renderer renderer;
    // MaterialPropertyBlock to set color in renderer
    private MaterialPropertyBlock propBlock;

    [SerializeField]
    private Color color = new Color(1, 1, 1, 1);

    // Color property to get/set color value
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

    void OnValidate()
    {
        ColorUpdate();
    }

    // Private method to update color of renderer
    private void ColorUpdate()
    {
        // Get renderer component
        if (renderer == null)
        {
            renderer = GetComponentInChildren<Renderer>();
        }
        // Get MaterialPropertyBlock
        if (propBlock == null)
        {
            propBlock = new MaterialPropertyBlock();
        }

        // Get property block from renderer
        renderer.GetPropertyBlock(propBlock);

        // Set color in property block
        propBlock.SetColor("_Color", color);

        // Apply property block to renderer
        renderer.SetPropertyBlock(propBlock);
    }

    // Awake method to initialize color
    private void Awake()
    {
        ColorUpdate();
    }
}


