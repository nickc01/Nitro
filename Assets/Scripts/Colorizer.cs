using Nitro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Colorizer : MonoBehaviour
{
	new Renderer renderer;
	MaterialPropertyBlock propBlock;


	[SerializeField]
	Color color = new Color(1, 1, 1, 1);

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

	void ColorUpdate()
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


