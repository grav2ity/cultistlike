using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Tooltip("Save to memory if not set")]
        public string fileName;

        private string memorySave;

        private Dictionary<int, CardViz> cards;

        public void Save(string json)
        {
            if (fileName != "")
            {
                SaveToFile(json);
            }
            else
            {
                memorySave = json;
            }
        }

        public string Load()
        {
            if (fileName != "")
            {
                return LoadFromFile();
            }
            else
            {
                return memorySave;
            }
        }

        public void SaveToFile(string s)
        {
            File.WriteAllText(fileName, s);
        }

        public string LoadFromFile()
        {
            return File.ReadAllText(fileName);
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

        private void Awake()
        {
            Instance = this;

            cards = new Dictionary<int, CardViz>();
        }
    }
}
