using System;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;


namespace CultistLike
{
    [Serializable]
    public class DeckInst
    {
        public Deck deck;
        [SerializeField] private List<Fragment> fragments;

        public DeckInst(Deck deck)
        {
            this.deck = deck;

            fragments = new List<Fragment>();
            Reshuffle();
        }

        public DeckInst(DeckInstSave save)
        {
            fragments = new();
            Load(save);
        }

        public Fragment Draw()
        {
            if (fragments.Count > 0)
            {
                if (deck.random)
                {
                    return Draw(Random.Range(0, fragments.Count));

                }
                else
                {
                    return Draw(0);
                }
            }
            else
            {
                if (deck.replenish)
                {
                    Reshuffle();
                    return Draw();
                }
                else
                {
                    return deck.defaultFragment;
                }
            }
        }

        private Fragment Draw(int i)
        {
            if (i >= 0 && i < fragments.Count)
            {
                var fragment = fragments[i];
                if (deck.infinite == false)
                {
                    fragments.RemoveAt(i);
                }
                return fragment;
            }
            else
            {
                return null;
            }
        }

        public Fragment DrawOffset(Fragment fragment, int di)
        {
            if (fragment != null)
            {
                var i = fragments.IndexOf(fragment);
                if (i != -1)
                {
                    return Draw(i + di);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public void Add(Fragment fragment)
        {
            if (fragment != null)
            {
                fragments.Add(fragment);
            }
        }

        public void AddFront(Fragment fragment)
        {
            if (fragment != null)
            {
                fragments.Insert(0, fragment);
            }
        }

        public DeckInstSave Save()
        {
            var save = new DeckInstSave()
            {
                deck = deck != null ? deck.SaveID() : 0,
                fragments = new()
            };

            foreach (var frag in fragments)
            {
                if (frag != null)
                    save.fragments.Add(frag.SaveID());
            }

            return save;
        }

        public void Load(DeckInstSave save)
        {
            deck = save.deck.SavedObject<Deck>();

            fragments.Clear();
            foreach (var frag in save.fragments)
            {
                fragments.Add(frag.SavedObject<Fragment>());
            }

        }

        private void Reshuffle()
        {
            if (deck.shuffle)
            {
                foreach (var fragment in deck.fragments)
                {
                    int r = Random.Range(0, fragments.Count);
                    fragments.Insert(r, fragment);
                }
            }
            else
            {
                fragments = deck.fragments.GetRange(0, deck.fragments.Count);
            }
        }
    }

    [Serializable]
    public class DeckInstSave
    {
        public uint deck;
        public List<uint> fragments;
    }
}
