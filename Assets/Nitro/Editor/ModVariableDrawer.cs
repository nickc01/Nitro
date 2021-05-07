using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IModVariable), true)]
public class ModVariableDrawer : PropertyDrawer
{
	bool firstRun = false;

	SerializedPropertyType type;

	TooltipAttribute Tooltip;


	void Init(SerializedProperty prop)
	{
		type = prop.propertyType;
		var tooltip = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), false);
		if (tooltip.GetLength(0) > 0)
		{
			Tooltip = (TooltipAttribute)tooltip[0];
		}
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (!firstRun)
		{
			firstRun = true;
			Init(property);
		}

		var valueProp = property.FindPropertyRelative("value");

		GUIContent content = null;

		if (Tooltip != null)
		{
			content = new GUIContent(property.displayName, Tooltip.tooltip);
		}
		else
		{
			content = new GUIContent(property.displayName);
		}
		EditorGUI.BeginDisabledGroup(Application.isPlaying);
		EditorGUI.PropertyField(position, valueProp, content);
		EditorGUI.EndDisabledGroup();
	}
}