using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using System.IO;

namespace CultistLike
{
    public class CultistAssets
    {
        public Dictionary<string, Act> acts = new();
        public Dictionary<string, Aspect> aspects = new();
        public Dictionary<string, Card> cards = new();
        public Dictionary<string, Rule> rules = new();
        public Dictionary<string, Slot> slots = new();
        public Dictionary<string, Token> tokens = new();

        static Dictionary<char, List<string>> typeToProps = new Dictionary<char, List<string>>
        {
            { 'A', new List<string>{"label", "text", "endText"} },
            { 'C', new List<string>{"label", "description"} },
            { 'F', new List<string>{"label", "description"} },
            { 'R', new List<string>{"text"} },
            { 'S', new List<string>{"label", "description"} },
            { 'T', new List<string>{"label", "description"} }
        };


        public void OnEntity(char entityType, string fileName, List<string> unnamedProperties,
                        Dictionary<char, string> namedProperties)
        {
            if (fileName == "")
            {
                return;
            }

            SerializedObject sobj = null;
            switch (entityType)
            {
                case 'A':
                    if (acts.ContainsKey(fileName))
                    {
                        sobj = new SerializedObject(acts[fileName]);
                    }
                    break;
                case 'C':
                    if (cards.ContainsKey(fileName))
                    {
                        sobj = new SerializedObject(cards[fileName]);
                    }
                    break;
                case 'F':
                    if (aspects.ContainsKey(fileName))
                    {
                        sobj = new SerializedObject(aspects[fileName]);
                    }
                    break;
                 case 'R':
                    if (rules.ContainsKey(fileName))
                    {
                        sobj = new SerializedObject(rules[fileName]);
                    }
                    break;
                 case 'S':
                    if (slots.ContainsKey(fileName))
                    {
                        sobj = new SerializedObject(slots[fileName]);
                    }
                    break;
                 case 'T':
                    if (tokens.ContainsKey(fileName))
                    {
                        sobj = new SerializedObject(tokens[fileName]);
                    }
                    break;
            }

            if (sobj != null)
            {
                sobj.Update();

                var propNames = typeToProps[entityType];
                if (propNames != null)
                {
                    for (int i=0; i < unnamedProperties.Count && i < propNames.Count; i++)
                    {
                        var prop = sobj.FindProperty(propNames[i]);
                        prop.stringValue = unnamedProperties[i];
                    }
                }

                sobj.ApplyModifiedProperties();
            }
        }
    }

    public class Importer : MonoBehaviour
    {
        static void FindAssets<T>(string type, Dictionary<string, T> dict) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + type, null);
            T asset;
            string fileName;
            foreach (string guid in guids)
            {
                fileName = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid));
                asset = (T)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(T));
                if (fileName != "")
                {
                    dict[fileName] = asset;
                }
            }
        }

        // [MenuItem("Cultistlike/Import Text")]
        static public void ImportText(string file)
        {
            CultistAssets assets = new();

            FindAssets("Act", assets.acts);
            FindAssets("Aspect", assets.aspects);
            FindAssets("Card", assets.cards);
            FindAssets("Rule", assets.rules);
            FindAssets("Slot", assets.slots);
            FindAssets("Token", assets.tokens);

            var parser = new EntityParser(
                (char entityType,
                string label,
                List<string> unnamedProperties,
                Dictionary<char, string> namedProperties) =>
            {
                Debug.Log($"Entity type: {entityType}");
                Debug.Log($"Label: {label}");

                foreach (var property in unnamedProperties)
                    Debug.Log($"Unnamed: {property}");

                foreach (var property in namedProperties)
                    Debug.Log($"Property {property.Key}: {property.Value}");

                    assets.OnEntity(entityType, label, unnamedProperties, namedProperties);
            });

            parser.ParseFile(file);
        }
    }
}