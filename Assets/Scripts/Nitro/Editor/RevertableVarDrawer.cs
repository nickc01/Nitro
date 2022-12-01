using Nitro;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IRevertableVar), true)]
public class RevertableVarDrawer : PropertyDrawer
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

    public override bool CanCacheInspectorGUI(SerializedProperty property)
    {
        return false;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!firstRun)
        {
            firstRun = true;
            Init(property);
        }

        var valueProp = property.FindPropertyRelative("value");
        var baseValueProp = property.FindPropertyRelative("baseValue");

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
        if (!Application.isPlaying)
        {
            SetPropertyValue(baseValueProp, GetPropertyValue(valueProp));
        }
        EditorGUI.EndDisabledGroup();
    }

    static object GetPropertyValue(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                return prop.intValue;
            case SerializedPropertyType.Boolean:
                return prop.boolValue;
            case SerializedPropertyType.Float:
                return prop.floatValue;
            case SerializedPropertyType.String:
                return prop.stringValue;
            case SerializedPropertyType.Color:
                return prop.colorValue;
            case SerializedPropertyType.ObjectReference:
                return prop.objectReferenceValue;
            case SerializedPropertyType.LayerMask:
                return (LayerMask)prop.intValue;
            case SerializedPropertyType.Enum:
                return prop.enumValueIndex;
            case SerializedPropertyType.Vector2:
                return prop.vector2Value;
            case SerializedPropertyType.Vector3:
                return prop.vector3Value;
            case SerializedPropertyType.Vector4:
                return prop.vector4Value;
            case SerializedPropertyType.Rect:
                return prop.rectValue;
            case SerializedPropertyType.ArraySize:
                return prop.arraySize;
            case SerializedPropertyType.Character:
                return (char)prop.intValue;
            case SerializedPropertyType.AnimationCurve:
                return prop.animationCurveValue;
            case SerializedPropertyType.Bounds:
                return prop.boundsValue;
            case SerializedPropertyType.Quaternion:
                return prop.quaternionValue;
            case SerializedPropertyType.ExposedReference:
                return prop.exposedReferenceValue;
            case SerializedPropertyType.Vector2Int:
                return prop.vector2IntValue;
            case SerializedPropertyType.Vector3Int:
                return prop.vector3IntValue;
            case SerializedPropertyType.RectInt:
                return prop.rectIntValue;
            case SerializedPropertyType.BoundsInt:
                return prop.boundsIntValue;
            default:
                return null;
        }
    }

    static void SetPropertyValue(SerializedProperty prop, object value)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                prop.intValue = (int)value;
                break;
            case SerializedPropertyType.Boolean:
                prop.boolValue = (bool)value;
                break;
            case SerializedPropertyType.Float:
                prop.floatValue = (float)value;
                break;
            case SerializedPropertyType.String:
                prop.stringValue = (string)value;
                break;
            case SerializedPropertyType.Color:
                prop.colorValue = (Color)value;
                break;
            case SerializedPropertyType.ObjectReference:
                prop.objectReferenceValue = value as Object;
                break;
            case SerializedPropertyType.LayerMask:
                prop.intValue = (value is LayerMask) ? ((LayerMask)value).value : (int)value;
                break;
            case SerializedPropertyType.Enum:
                prop.enumValueIndex = (int)value;
                break;
            case SerializedPropertyType.Vector2:
                prop.vector2Value = (Vector2)value;
                break;
            case SerializedPropertyType.Vector3:
                prop.vector3Value = (Vector3)value;
                break;
            case SerializedPropertyType.Vector4:
                prop.vector4Value = (Vector3)value;
                break;
            case SerializedPropertyType.Rect:
                prop.rectValue = (Rect)value;
                break;
            case SerializedPropertyType.ArraySize:
                prop.arraySize = (int)value;
                break;
            case SerializedPropertyType.Character:
                prop.intValue = (int)value;
                break;
            case SerializedPropertyType.AnimationCurve:
                prop.animationCurveValue = value as AnimationCurve;
                break;
            case SerializedPropertyType.Bounds:
                prop.boundsValue = (Bounds)value;
                break;
            case SerializedPropertyType.Quaternion:
                prop.quaternionValue = (Quaternion)value;
                break;
            case SerializedPropertyType.ExposedReference:
                prop.exposedReferenceValue = (Object)value;
                break;
            case SerializedPropertyType.Vector2Int:
                prop.vector2IntValue = (Vector2Int)value;
                break;
            case SerializedPropertyType.Vector3Int:
                prop.vector3IntValue = (Vector3Int)value;
                break;
            case SerializedPropertyType.RectInt:
                prop.rectIntValue = (RectInt)value;
                break;
            case SerializedPropertyType.BoundsInt:
                prop.boundsIntValue = (BoundsInt)value;
                break;
            default:
                break;
        }
    }
}
