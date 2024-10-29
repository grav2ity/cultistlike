﻿using System;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;


namespace CultistLike
{
    [Serializable]
    public class DeckInst
    {
        [SerializeField] public Deck deck;
        [SerializeField] private List<Fragment> fragments;
        [SerializeField] private int index;

        public DeckInst(Deck deck)
        {
            this.deck = deck;

            index = 0;

            if (deck.shuffle == true)
            {
                fragments = new List<Fragment>();
                for (int i=0; i<deck.fragments.Count; i++)
                {
                    int r = Random.Range(0, fragments.Count);
                    fragments.Insert(r, deck.fragments[i]); 
                }
            }
            else
            {
                fragments = new List<Fragment>(deck.fragments);
            }
        }

        public Fragment Draw() => Draw(0);

        public Fragment Draw(int i)
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
    }
}