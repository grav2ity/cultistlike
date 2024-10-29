using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [Serializable]
    public class Context : IDisposable
    {
        public ActLogic actLogic;
        public ActLogic parent;

        public FragContainer source;
        public FragContainer parentSource;

        public FragContainer scope;
        public FragContainer parentScope;

        public List<CardViz> matches;

        public CardViz card;

        public List<ActModifierC> actModifiers = new List<ActModifierC>();
        public List<CardModifierC> cardModifiers = new List<CardModifierC>();
        public List<TableModifier> tableModifiers = new List<TableModifier>();
        public List<PathModifier> pathModifiers = new List<PathModifier>();
        public List<DeckModifier> deckModifiers = new List<DeckModifier>();

        private List<CardViz> toDestroy = new List<CardViz>();


        public Context(FragContainer fragments, bool keepMatches = false)
        {
            if (fragments != null)
            {
                scope = fragments;
                source = fragments;

                if (keepMatches == true)
                {
                    matches = source.matches;
                }
                else
                {
                    matches = source.cards.GetRange(0, fragments.cards.Count);
                }
            }
        }

        public Context(ActLogic actLogic, bool keepMatches = false) : this(actLogic.fragments, keepMatches)
        {
            if (actLogic != null)
            {
                this.actLogic = actLogic;

                if (actLogic.parent != null)
                {
                    this.parent = actLogic.parent;
                    parentSource = actLogic.parent.fragments;
                    parentScope = actLogic.parent.fragments;
                }
            }
        }

        public Context(CardViz cardViz, bool keepMatches = false) : this(cardViz.fragments, keepMatches)
        {
            if (cardViz != null)
            {
                card = cardViz;
                scope.AddCardVizOnly(cardViz);
            }
        }

        public void Dispose()
        {
            foreach (var actModifier in actModifiers)
            {
                actModifier.Execute(this);
            }
            foreach (var cardModifier in cardModifiers)
            {
                cardModifier.Execute(this);
            }
            foreach (var tableModifier in tableModifiers)
            {
                tableModifier.Execute(this);
            }
            foreach (var pathModifier in pathModifiers)
            {
                pathModifier.Execute(this);
            }
            foreach (var deckModifier in deckModifiers)
            {
                deckModifier.Execute(this);
            }

            foreach (var cardViz in toDestroy)
            {
                GameManager.Instance.DestroyCard(cardViz);
            }
        }

        public void Destroy(HeldFragment frag)
        {
            if (frag.cardViz != null)
            {
                toDestroy.Add(frag.cardViz);
            }
        }

        public void ResetMatches()
        {
            matches = source.cards.GetRange(0, actLogic.fragments.cards.Count);
        }

        public void SaveMatches()
        {
            scope.matches = matches;
        }

        public HeldFragment Resolve(Fragment frag)
        {
            if (frag != null)
            {
                if (frag == GameManager.Instance.thisCard)
                {
                    return new HeldFragment(card);
                }
                if (frag == GameManager.Instance.matchedCard)
                {
                    if (matches.Count > 0)
                    {
                        return new HeldFragment(matches[0]);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return new HeldFragment(frag);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
