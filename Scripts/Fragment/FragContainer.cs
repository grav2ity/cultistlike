using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [Serializable]
    public class FragContainer
    {
        public List<CardViz> cards;
        public List<HeldFragment> fragments;
        public List<CardViz> matches;


        public void Clear()
        {
            cards.Clear();
            matches.Clear();

            fragments.Clear();
        }

        public void Add(Fragment frag) => frag?.AddToContainer(this);
        public void Remove(Fragment frag) => frag?.RemoveFromContainer(this);
        public void Adjust(Fragment frag, int level) => frag?.AdjustInContainer(this, level);
        public int Count(Fragment frag) => frag.CountInContainer(this);


        public void Add(Aspect aspect)
        {
            if (aspect != null)
            {
                aspect.AddToList(fragments);
            }
        }

        public void Add(HeldFragment frag)
        {
            if (frag != null)
            {
                if (frag.cardViz != null)
                {
                    Add(frag.cardViz);
                }
                else
                {
                    frag.AddToList(fragments);
                }
            }
        }

        public void Add(Card card)
        {
            if (card != null)
            {
                var cardViz = GameManager.Instance.CreateCard(card);
                Add(cardViz);
            }
        }

        public void Add(CardViz cardViz)
        {
            if (cardViz != null)
            {
                cards.Add(cardViz);
                foreach (var frag in cardViz.fragments.fragments)
                {
                    Add(frag);
                }
            }
        }

        public void AddCardVizOnly(CardViz cardViz)
        {
            if (cardViz != null)
            {
                cards.Add(cardViz);
            }
        }

        public void Adjust(Aspect aspect, int level)
        {
            if (aspect != null)
            {
                aspect.AdjustInList(fragments, level);
            }
        }

        public void Adjust(HeldFragment frag, int level)
        {
            if (frag.cardViz != null)
            {
                Adjust(frag.cardViz, level);
            }
            else
            {
                Adjust(frag.fragment, level);
            }
        }

        public void Adjust(Card card, int level)
        {
            if (card != null)
            {
                if (level > 0)
                {
                    while (level-- > 0)
                    {
                        Add(card);
                    }
                }
                else if (level < 0)
                {
                    while (level++ < 0)
                    {
                        Remove(card);
                    }
                }
            }
        }

        public void Adjust(CardViz card, int level)
        {
            if (card != null)
            {
                if (level > 0)
                {
                    while (level-- > 0)
                    {
                        Add(card);
                    }
                }
                else if (level < 0)
                {
                    while (level++ < 0)
                    {
                        Remove(card);
                    }
                }
            }
        }

        public void Remove(Aspect aspect)
        {
            if (aspect != null)
            {
                aspect.RemoveFromList(fragments);
            }
        }

        public void Remove(HeldFragment frag)
        {
            if (frag != null)
            {
                if (frag.cardViz != null)
                {
                    Remove(frag.cardViz);
                }
                else
                {
                    frag.RemoveFromList(fragments);
                }
            }
        }

        public CardViz Remove(Card card)
        {
            if (card != null)
            {
                var cardViz = cards.Find(x => x.card == card);
                if (cardViz != null)
                {
                    Remove(cardViz);
                }
                return cardViz;
            }
            return null;
        }

        public CardViz Remove(CardViz cardViz)
        {
            if (cardViz != null)
            {
                cards.Remove(cardViz);
                matches.Remove(cardViz);
                foreach (var frag in cardViz.fragments.fragments)
                {
                    Remove(frag);
                }
            }
            return cardViz;
        }

        public HeldFragment Find(Aspect aspect) =>
            fragments.Find(x => x.fragment == aspect);

        // public HeldAspect Find(HeldAspect ha) =>
        //     aspects.Find(x => x.aspect == ha.aspect && x.count >= ha.count);

        // public HeldAspect Find(Aspect a, Predicate<int> p) =>
        //     aspects.Find(x => x.aspect == a && p(x.count) == true);

        // public HeldAspect Find(HeldAspect ha, Predicate<int> p) =>
        //     aspects.Find(x => x.aspect == ha.aspect && p(x.count) == true);

        public CardViz Find(Card card) => cards.Find(x => x.card == card);
        public List<CardViz> FindAll(Card card) => cards.FindAll(x => x.card == card);

        public List<CardViz> FindAll(Aspect aspect) =>
            cards.FindAll(x => x.fragments.Count(aspect) > 0);

        public int Count(Aspect aspect)
        {
            var a = Find(aspect);
            return (a != null ? a.count : 0);
        }

        public int Count(Card card) => cards.FindAll(x => x.card == card).Count;

        public int Count(HeldFragment frag)
        {
            if (frag != null)
            {
                if (frag.cardViz != null)
                {
                    return Count(frag.cardViz.card);
                }
                else
                {
                    return Count(frag.fragment);
                }
            }
            else
            {
                return 0;
            }
        }
    }
}
