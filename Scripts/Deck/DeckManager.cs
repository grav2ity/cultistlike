using System.Collections.Generic;

using UnityEngine;



namespace CultistLike
{
    public class DeckManager : MonoBehaviour
    {
        public static DeckManager Instance { get; private set; }

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

        public void Load(List<DeckInst> decks)
        {
            deckInsts = decks;
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
            Instance = this;

            decDict = new Dictionary<Deck, DeckInst>();
        }
    }
}
