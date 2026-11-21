using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public class DeckManager : MonoBehaviour, ISerializationCallbackReceiver
    {
        private static DeckManager _instance;

        public static DeckManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = FindObjectOfType<DeckManager>();
                }

                return _instance;
            }
        }

        public List<DeckInst> deckInsts;

        private Dictionary<Deck, DeckInst> decDict;


        public DeckInst GetDeckInst(Deck deck)
        {
            if (decDict.TryGetValue(deck, out var inst))
            {
                return inst;
            }
            else
            {
                var deckInst = new DeckInst(deck);
                decDict[deck] = deckInst;
                deckInsts.Add(deckInst);
                return deckInst;
            }
        }

        public List<DeckInstSave> Save()
        {
            var save = new List<DeckInstSave>();
            foreach (var deckInst in deckInsts)
            {
                save.Add(deckInst.Save());
            }
            return save;
        }

        public void Load(List<DeckInstSave> decks)
        {
            deckInsts.Clear();
            foreach (var deckInstSave in decks)
            {
                deckInsts.Add(new DeckInst(deckInstSave));
            }

            decDict.Clear();
            foreach (var deckInst in deckInsts)
            {
                decDict[deckInst.deck] = deckInst;
            }
        }

        public void Reset()
        {
            deckInsts.Clear();
            decDict.Clear();
        }

        private void Awake()
        {
            decDict = new Dictionary<Deck, DeckInst>();
        }

        //temp stuff for built-in hot reload
        private List<Deck> decks = new();
        private List<DeckInst> dInstsList = new();

        public virtual void OnBeforeSerialize()
        {
            decks.Clear();
            dInstsList.Clear();
            foreach (var kvp in decDict)
            {
                decks.Add(kvp.Key);
                dInstsList.Add(kvp.Value);
            }
        }

        public virtual void OnAfterDeserialize()
        {
            decDict = new();
            for (int i = 0; i != Math.Min(decks.Count, dInstsList.Count); i++)
                decDict.Add(decks[i], dInstsList[i]);
        }
    }
}
