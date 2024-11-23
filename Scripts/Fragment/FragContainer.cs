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

        public FragContainer parent;

        public Action<CardViz> onCreateCard;


        public void Clear()
        {
            cards.Clear();
            matches.Clear();

            fragments.Clear();
        }

        public void Add(Fragment frag) => frag.AddToContainer(this);
        public void Remove(Fragment frag) => frag.RemoveFromContainer(this);
        public int Adjust(Fragment frag, int level) => frag.AdjustInContainer(this, level);
        public int Count(Fragment frag) => frag.CountInContainer(this);

        public void Add(Aspect aspect) => Adjust(aspect, 1);
        public void Remove(Aspect aspect) => Adjust(aspect, -1);
        public int Adjust(Aspect aspect, int level)
        {
            if (aspect != null)
            {
                int adjustedL = aspect.AdjustInList(fragments, level);
                parent?.Adjust(aspect, adjustedL);
                return adjustedL;
            }
            else
            {
                return 0;
            }
        }
        public int Count(Aspect aspect)
        {
            var a = Find(aspect);
            return (a != null ? a.count : 0);
        }


        public void Add(HeldFragment frag) => frag?.AddToList(fragments);

        public void Add(Card card)
        {
            if (card != null)
            {
                var cardViz = GameManager.Instance.CreateCard(card);
                Add(cardViz);
                if (onCreateCard != null)
                {
                    onCreateCard(cardViz);
                }
            }
        }

        public void Add(CardViz cardViz)
        {
            if (cardViz != null && cards.Contains(cardViz) == false)
            {
                AddCardVizOnly(cardViz);

                foreach (var frag in cardViz.fragments.fragments)
                {
                    Add(frag);
                }

                cardViz.fragments.parent = this;
            }
        }

        public void AddCardVizOnly(CardViz cardViz)
        {
            if (cardViz != null && cards.Contains(cardViz) == false)
            {
                cards.Add(cardViz);
            }
        }

        public int Adjust(Target target, int level)
        {
            if (target != null)
            {
                if (target.cards != null)
                {
                    foreach (var cardViz in target.cards)
                    {
                        Adjust(cardViz, level);
                    }
                }
                else if (target.fragment != null)
                {
                    return Adjust(target.fragment, level);
                }
            }
            return 0;
        }

        public int Adjust(Card card, int level)
        {
            if (card != null)
            {
                if (level > 0)
                {
                    int count = level;
                    while (level-- > 0)
                    {
                        Add(card);
                    }
                    return count;
                }
                else if (level < 0)
                {
                    int count = 0;
                    while (level++ < 0)
                    {
                        if (Remove(card) != null)
                        {
                            count--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    return count;
                }
            }
            return 0;
        }

        public int Adjust(CardViz cardViz, int level)
        {
            if (cardViz != null)
            {
                if (level > 0)
                {
                    int count = level;
                    // Add(cardViz);
                    // level--;

                    while (level-- > 0)
                    {
                        var newCardViz = cardViz.Duplicate();
                        Add(newCardViz);
                        if (onCreateCard != null)
                        {
                            onCreateCard(newCardViz);
                        }
                    }
                    return count;
                }
                else if (level < 0)
                {
                    if (Remove(cardViz) != null)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            return 0;
        }

        public void Remove(HeldFragment frag) => frag?.RemoveFromList(fragments);

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
            if (cardViz != null && cards.Contains(cardViz) == true)
            {
                RemoveCardVizOnly(cardViz);

                foreach (var frag in cardViz.fragments.fragments)
                {
                    Remove(frag);
                }

                if (cardViz.fragments.parent == this)
                {
                    cardViz.fragments.parent = null;
                }
            }
            return cardViz;
        }

        public CardViz RemoveCardVizOnly(CardViz cardViz)
        {
            if (cardViz != null)
            {
                cards.Remove(cardViz);
                //TODO ??
                matches.Remove(cardViz);
            }
            return cardViz;
        }

        public HeldFragment Find(Aspect aspect) =>
            fragments.Find(x => x.fragment == aspect);

        public CardViz Find(Card card) => cards.Find(x => x.card == card);
        public List<CardViz> FindAll(Card card) => cards.FindAll(x => x.card == card);

        public List<CardViz> FindAll(Aspect aspect) =>
            cards.FindAll(x => x.fragments.Count(aspect) > 0);

        public int Count(Card card) => cards.FindAll(x => x.card == card).Count;

        public int Count(HeldFragment frag) => frag != null ? Count(frag.fragment) : 0;
    }
}
