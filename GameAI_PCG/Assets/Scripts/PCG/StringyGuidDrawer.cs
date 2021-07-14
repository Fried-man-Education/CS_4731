using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(StringyGuid))]
public class StringyGuidDrawer : PropertyDrawer
{
    bool DO_VALIDATION = true;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var storageProp = property.FindPropertyRelative("m_storage");
        if (DO_VALIDATION)
        {
            var oldval = storageProp.stringValue;
            var newval = EditorGUI.DelayedTextField(position, oldval);
            if (oldval != newval)
            {
                try
                {
                    storageProp.stringValue = new System.Guid(newval).ToString("D");
                }
                catch (System.FormatException) { }
            }
        }
        else
        {
            EditorGUI.PropertyField(position, storageProp, GUIContent.none);
        }
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
#endif

[System.Serializable]
public struct StringyGuid
{
    [SerializeField]
    private string m_storage;

    public static implicit operator StringyGuid(System.Guid rhs)
    {
        return new StringyGuid { m_storage = rhs.ToString("D") };
    }

    public static implicit operator System.Guid(StringyGuid rhs)
    {
        if (rhs.m_storage == null) return System.Guid.Empty;
        try
        {
            return new System.Guid(rhs.m_storage);
        }
        catch (System.FormatException)
        {
            return System.Guid.Empty;
        }
    }
    public override string ToString()
    {
        return new System.Guid(this.m_storage).ToString();
    }
}