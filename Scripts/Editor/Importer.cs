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

        public void OnEntity(char entityType, string fileName, List<string> unnamedProperties,
                        Dictionary<char, string> namedProperties)
        {
            if (fileName == "")
            {
                return;
            }

            switch (entityType)
            {
                case 'A':
                    if (acts.ContainsKey(fileName))
                    {
                        if (unnamedProperties.Count > 0)
                        {
                            acts[fileName].label = unnamedProperties[0];
                        }
                        if (unnamedProperties.Count > 1)
                        {
                            acts[fileName].text = unnamedProperties[1];
                        }
                        if (unnamedProperties.Count > 2)
                        {
                            acts[fileName].endText = unnamedProperties[2];
                        }
                    }
                    break;
                case 'C':
                    if (cards.ContainsKey(fileName))
                    {
                        if (unnamedProperties.Count > 0)
                        {
                            cards[fileName].label = unnamedProperties[0];
                        }
                        if (unnamedProperties.Count > 1)
                        {
                            cards[fileName].description = unnamedProperties[1];
                        }
                    }
                    break;
                case 'F':
                    if (aspects.ContainsKey(fileName))
                    {
                        if (unnamedProperties.Count > 0)
                        {
                            aspects[fileName].label = unnamedProperties[0];
                        }
                        if (unnamedProperties.Count > 1)
                        {
                            aspects[fileName].description = unnamedProperties[1];
                        }
                    }
                    break;
                 case 'R':
                    if (rules.ContainsKey(fileName))
                    {
                        if (unnamedProperties.Count > 0)
                        {
                            rules[fileName].text = unnamedProperties[0];
                        }
                    }
                    break;
                 case 'S':
                    if (slots.ContainsKey(fileName))
                    {
                        if (unnamedProperties.Count > 0)
                        {
                            slots[fileName].description = unnamedProperties[0];
                        }
                    }
                    break;
                 case 'T':
                    if (tokens.ContainsKey(fileName))
                    {
                        if (unnamedProperties.Count > 0)
                        {
                            tokens[fileName].description = unnamedProperties[0];
                        }
                    }
                    break;
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

        [MenuItem("Cultistlike/Import Text")]
        static void ImportText()
        {
            CultistAssets assets = new();

            FindAssets<Act>("Act", assets.acts);
            FindAssets<Aspect>("Aspect", assets.aspects);
            FindAssets<Card>("Card", assets.cards);
            FindAssets<Rule>("Rule", assets.rules);
            FindAssets<Slot>("Slot", assets.slots);
            FindAssets<Token>("Token", assets.tokens);

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

            parser.ParseFile("data.txt");
        }
    }
}