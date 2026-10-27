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

            SerializedProperty property =
                serializedObject.GetIterator();

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
}