using System.Collections.Generic;
using System.Linq;

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
            if (decDict.ContainsKey(deck) == true)
            {
                return decDict[deck];
            }
            else
            {
                var deckInst = new DeckInst(deck);
                decDict[deck] = deckInst;
                deckInsts.Add(deckInst);
                return deckInst;
            }
        }


        private void PopulateDict()
        {
            var decks = new List<Deck>(Resources.LoadAll("", typeof(Deck)).Cast<Deck>().ToArray());

            foreach (var deck in decks)
            {
                var d = deckInsts.Find(x => x.deck == deck);
                if (d != null)
                {
                    decDict[deck] = d;
                }
            }
        }

        private void Awake()
        {
            Instance = this;

            decDict = new Dictionary<Deck, DeckInst>();
            PopulateDict();
        }
    }
}
