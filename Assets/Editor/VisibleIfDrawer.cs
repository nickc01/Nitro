/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class VisibleIfDrawer : PropertyDrawer
{
	bool init = false;

	VisibleIfAttribute data;

	FieldInfo firstField;
	FieldInfo secondField;

	Type sourceType;

	object secondValue;

	void Init()
	{
		data = (VisibleIfAttribute)attribute;
		sourceType = fieldInfo.DeclaringType;

		var firstField = sourceType.GetField(data.FieldName, BindingFlags.Public | BindingFlags.Instance);
		if (firstField == null)
		{
			firstField = sourceType.GetField(data.FieldName, BindingFlags.NonPublic | BindingFlags.Instance);
		}
		if (firstField == null)
		{
			throw new Exception($"Could not find the field {data.FieldName} on type {sourceType}");
		}

		if (data.FieldToCompareTo != null)
		{
			secondField = sourceType.GetField(data.FieldToCompareTo, BindingFlags.Public | BindingFlags.Instance);
			if (secondField == null)
			{
				secondField = sourceType.GetField(data.FieldToCompareTo, BindingFlags.NonPublic | BindingFlags.Instance);
			}
			if (secondField == null)
			{
				throw new Exception($"Could not find the field {data.FieldToCompareTo} on type {sourceType}");
			}
		}
		else
		{
			secondValue = data.ValueToCompareTo;
		}

	}

	bool ShouldBeVisible
	{
		get
		{
			if (secondField != null)
			{

			}
			else
			{
				var firstValue = 
				switch (data.Operation)
				{
					case VisibilityOperation.EqualTo:
						return secondValue.Equals();
						break;
					case VisibilityOperation.NotEqualTo:
						break;
					case VisibilityOperation.GreaterThan:
						break;
					case VisibilityOperation.LessThan:
						break;
					case VisibilityOperation.GreaterThanOrEqual:
						break;
					case VisibilityOperation.LessThanOrEqual:
						break;
					default:
						break;
				}
			}
		}
	}


	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (!init)
		{
			init = true;
			Init();
		}
		base.OnGUI(position, property, label);
	}
}

*/