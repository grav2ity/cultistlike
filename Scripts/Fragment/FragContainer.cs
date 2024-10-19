using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [Serializable]
    public class FragContainer
    {
        public List<CardViz> cards;
        public List<HeldAspect> aspects;

        public List<CardViz> matches;


        public FragContainer()
        {
            cards = new List<CardViz>();
            aspects = new List<HeldAspect>();
            matches = new List<CardViz>();
        }


        public void Clear()
        {
            cards.Clear();
            aspects.Clear();
            matches.Clear();
        }

        public void Add(Fragment frag) => frag.AddToContainer(this);
        public void Adjust(Fragment frag, int level) => frag.AdjustInContainer(this, level);
        public void Remove(Fragment frag, int level) => frag.RemoveFromContainer(this);
        public int Count(Fragment frag) => frag.CountInContainer(this);


        public void Add(Aspect aspect)
        {
            if (aspect != null)
            {
                aspect.AddToList(aspects);
            }
        }

        public void Add(HeldAspect aspect)
        {
            if (aspect != null)
            {
                aspect.AddToList(aspects);
            }
        }

        public void Add(Card card)
        {
            if (card != null)
            {
                var cardViz = UnityEngine.Object.Instantiate(GameManager.Instance.cardPrefab);
                cardViz.SetCard(card);
                Add(cardViz);
            }
        }

        public void Add(CardViz cardViz)
        {
            if (cardViz != null)
            {
                cards.Add(cardViz);
                foreach (var aspect in cardViz.fragments.aspects)
                {
                    Add(aspect);
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
            if (level < 0)
            {
                for (int i=level; i<0; i++)
                {
                    DestroyCardWithAspect(aspect);
                }
            }
            aspect.AdjustInList(aspects, level);
        }

        public void Addjust(Card card, int level)
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

        public void DestroyCard(CardViz cardViz)
        {
            cards.Remove(cardViz);
            matches.Remove(cardViz);
            GameManager.Instance.DestroyCard(cardViz);
        }

        private void DestroyCardWithAspect(Aspect aspect)
        {
            var cardViz = cards.Find(x => x.card.fragments.Contains(aspect) != false);
            if (cardViz != null)
            {
                DestroyCard(cardViz);
            }
        }

        public void Remove(Aspect aspect)
        {
            if (aspect != null)
            {
                aspect.RemoveFromList(aspects);
            }
        }

        public void Remove(HeldAspect aspect)
        {
            if (aspect != null)
            {
                aspect.RemoveFromList(aspects);
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
                foreach (var aspect in cardViz.fragments.aspects)
                {
                    Remove(aspect);
                }
            }
            return cardViz;
        }

        public HeldAspect Find(Aspect aspect) =>
            aspects.Find(x => x.aspect == aspect);

        public HeldAspect Find(HeldAspect ha) =>
            aspects.Find(x => x.aspect == ha.aspect && x.count >= ha.count);

        public HeldAspect Find(Aspect a, Predicate<int> p) =>
            aspects.Find(x => x.aspect == a && p(x.count) == true);

        public HeldAspect Find(HeldAspect ha, Predicate<int> p) =>
            aspects.Find(x => x.aspect == ha.aspect && p(x.count) == true);

        public CardViz Find(Card card) => cards.Find(x => x.card == card);

        public int Count(Aspect aspect)
        {
            var a = Find(aspect);
            return (a != null ? a.count : 0);
        }

        public int Count(Card card) => cards.FindAll(x => x.card == card).Count;
    }
}
