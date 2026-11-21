using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;


namespace CultistLike
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;

        public static SaveManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = FindObjectOfType<SaveManager>();
                }

                return _instance;
            }
        }


        public SaveDBase idBase;

        [Tooltip("Save to memory if not set")]
        public string fileName;

        [Header("Autosave")]
        public float saveFrequency;
        public int maxSaves;


        private Dictionary<int, CardViz> cards;

        private float nextAutoSave;
        [SerializeField, HideInInspector] private int autoSaveIndex;

        public void SaveAuto()
        {
            // if (autoSaveIndex > maxSaves - 1)
            // {
            //     autoSaveIndex = 0;
            // }
            // else if (autoSaveIndex > idBase.autoSaves.Count - 1)
            // {
            //     idBase.autoSaves.Add(new Save());
            // }
            // idBase.autoSaves[autoSaveIndex].json = GameManager.Instance.Save();
            // autoSaveIndex++;
        }

        public void LoadAuto(int i)
        {
            if (i >= 0 && i < idBase.autoSaves.Count)
            {
                GameManager.Instance.Load(idBase.autoSaves[i].json);
            }
            GameManager.Instance.SetTimeScale(0f);
        }

        public void Save()
        {
            var json = GameManager.Instance.Save();
            if (fileName != "")
            {
                SaveToFile(json);
            }
            else
            {
                idBase.defaultSave.json = json;
            }
            EditorUtility.SetDirty(idBase);
        }

        public void Load()
        {
            string json;
            if (fileName != "")
            {
                json = LoadFromFile();
            }
            else
            {
                json = idBase.defaultSave.json;
            }
            GameManager.Instance.Load(json);
        }

        public void SaveToFile(string s)
        {
            File.WriteAllText(fileName, s);
        }

        public string LoadFromFile()
        {
            return File.ReadAllText(fileName);
        }

        public uint NewID(ScriptableObject so)
        {
            idBase.sos.Add(so);
            return (uint)idBase.sos.Count;
        }

        public void RegisterCard(int i, CardViz cardViz)
        {
            if (cardViz != null)
            {
                cards[i] = cardViz;
            }
        }

        public CardViz CardFromID(int i)
        {
            if (cards.ContainsKey(i))
            {
                return cards[i];
            }
            else
            {
                return null;
            }
        }

        private void OnEnable()
        {
            cards = new Dictionary<int, CardViz>();
            nextAutoSave = saveFrequency;
        }

        private void Update()
        {
            nextAutoSave -= GameManager.Instance.DeltaTime;
            if (nextAutoSave < 0f)
            {
                nextAutoSave = saveFrequency + nextAutoSave;
                SaveAuto();
            }
        }
    }

    [Serializable]
    public class Save
    {
        public string name;
        public string json;
    }
}
