using UnityEditor;
using UnityEngine;


namespace CultistLike
{
    [CustomEditor(typeof(Act))]
    public class ActEditor : CultistEditor {}

    [CustomEditor(typeof(Aspect))]
    public class AspectEditor : CultistEditor {}

    [CustomEditor(typeof(Card))]
    public class CardEditor : CultistEditor {}

    [CustomEditor(typeof(Deck))]
    public class DeckEditor : CultistEditor {}

    [CustomEditor(typeof(Rule))]
    public class RuleEditor : CultistEditor {}

    [CustomEditor(typeof(Slot))]
    public class SlotEditor : CultistEditor {}

    [CustomEditor(typeof(Token))]
    public class TokenEditor : CultistEditor {}


    public class CultistEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty property = serializedObject.GetIterator();

            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (property.name == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(property, true);
                    }
                }
                else if (property.propertyType == SerializedPropertyType.ObjectReference &&
                         property.objectReferenceValue != null)
                {
                    Color previousColor = GUI.color;
                    GUI.color = Color.green;
                    EditorGUILayout.PropertyField(property, true);
                    GUI.color = previousColor;
                }
                else if (property.propertyType == SerializedPropertyType.Float &&
                         property.floatValue != 0f)
                {
                    Color previousColor = GUI.color;
                    GUI.color = Color.green;
                    EditorGUILayout.PropertyField(property, true);
                    GUI.color = previousColor;
                }
                else if (property.propertyType == SerializedPropertyType.Boolean &&
                         property.boolValue == true)
                {
                    Color previousColor = GUI.color;
                    GUI.color = Color.green;
                    EditorGUILayout.PropertyField(property, true);
                    GUI.color = previousColor;
                }
                else if (property.isArray && property.propertyType != SerializedPropertyType.String)
                {
                    SerializedProperty size = property.FindPropertyRelative("Array.size");

                    if (size != null )
                    {
                        Color previousColor = GUI.color;

                        if (size.intValue > 0 && property.isExpanded == false)
                        {
                            GUI.color = Color.green;
                        }
                        else if (size.intValue > 0 && property.isExpanded == true)
                        {
                            GUI.color = new Color(0.75f, 1f, 1f, 1f);
                        }

                        EditorGUILayout.PropertyField(property, true);
                        GUI.color = previousColor;
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SaveManager))]
    public class SaveEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            GUILayout.Space(40);

            SaveManager sm =  serializedObject.targetObject as SaveManager;
            var idb = new SerializedObject(sm.idBase);
            var myArray = idb.FindProperty("saves");

            for (int i = 0; i < myArray.arraySize; i++)
            {
                EditorGUILayout.PropertyField(
                    myArray.GetArrayElementAtIndex(i),
                    new GUIContent(myArray.GetArrayElementAtIndex(i).displayName)
                );
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Save", GUILayout.Width(70)))
                {
                    var save = GameManager.Instance.Save();
                    //need this or ids generated during save won't be saved
                    idb.Update();
                    myArray.GetArrayElementAtIndex(i).FindPropertyRelative("json").stringValue = save;
                }
                if (GUILayout.Button("Load", GUILayout.Width(70)))
                {
                    GameManager.Instance.Load( myArray.GetArrayElementAtIndex(i).FindPropertyRelative("json").stringValue);
                }
                EditorGUILayout.EndHorizontal();
            }

            idb.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SaveDBase))]
    public class SaveDBaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(10);

            if (GUILayout.Button("Generate IDs"))
            {
                serializedObject.Update();
                SaveDBase idBase = (SaveDBase)serializedObject.targetObject;
                idBase.GenerateIDs();
            }
            if (GUILayout.Button("Reset IDs"))
            {
                serializedObject.Update();
                SaveDBase idBase = (SaveDBase)serializedObject.targetObject;
                idBase.ResetIDs();
            }
        }
    }
}