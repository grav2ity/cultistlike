using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace CultistLike
{
    public class SaveDBase : ScriptableObject
    {
        public List<ScriptableObject> sos;

        public List<Save> saves;
        public List<Save> autoSaves;
        public Save defaultSave;

        static void FindAssets<T>(string type, List<T> assets) where T : Object
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
                   assets.Add(asset);
                }
            }
        }

        public void GenerateIDs<T>(string type) where T : Object
        {
            List<T> ts = new();
            FindAssets<T>(type, ts);

            var thisS = new SerializedObject(this);
            var sosS = thisS.FindProperty("sos");

            foreach (var t in ts)
            {
                var so = new SerializedObject(t);
                so.Update();
                if (so.FindProperty("ID").uintValue == 0)
                {
                    sosS.InsertArrayElementAtIndex(sosS.arraySize);
                    sosS.GetArrayElementAtIndex(sosS.arraySize - 1).objectReferenceValue = t;

                    so.FindProperty("ID").uintValue = (uint)sosS.arraySize;
                    so.ApplyModifiedProperties();
                }
            }

            thisS.ApplyModifiedProperties();
        }

        public void GenerateIDs()
        {
            GenerateIDs<Act>("Act");
            GenerateIDs<Aspect>("Aspect");
            GenerateIDs<Card>("Card");
            GenerateIDs<Deck>("Deck");
            GenerateIDs<Slot>("Slot");
            GenerateIDs<Token>("Token");
        }

        public void ResetIDs<T>(string type) where T : Object
        {
            List<T> ts = new();
            FindAssets(type, ts);
            foreach (var t in ts)
            {
                var so = new SerializedObject(t);
                so.FindProperty("ID").uintValue = 0;
                so.ApplyModifiedProperties();
            }
        }

        public void ResetIDs()
        {
            var thisS = new SerializedObject(this);
            var sosS = thisS.FindProperty("sos");

            sosS.ClearArray();

            ResetIDs<Act>("Act");
            ResetIDs<Aspect>("Aspect");
            ResetIDs<Card>("Card");
            ResetIDs<Deck>("Deck");
            ResetIDs<Slot>("Slot");
            ResetIDs<Token>("Token");

            thisS.ApplyModifiedProperties();
        }
    }
}
