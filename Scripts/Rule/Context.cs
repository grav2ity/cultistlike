﻿using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    /// <summary>
    /// Accumulates modifiers and applies them on Dispose.
    /// Keeps track of scope and matched cards.
    /// </summary>
    public class Context : IDisposable
    {
        public ActLogic actLogic;
        public ActLogic parent;

        public FragContainer scope;
        public FragContainer parentScope;

        public Fragment thisAspect;
        public CardViz thisCard;
        public List<CardViz> matches;

        public List<ActModifierC> actModifiers = new List<ActModifierC>();
        public List<CardModifierC> cardModifiers = new List<CardModifierC>();
        public List<TableModifier> tableModifiers = new List<TableModifier>();
        public List<PathModifier> pathModifiers = new List<PathModifier>();
        public List<DeckModifierC> deckModifiers = new List<DeckModifierC>();

        private List<CardViz> toDestroy = new List<CardViz>();


        public Context(Context context, bool keepMatches = false) : this(context.actLogic, keepMatches) {}

        public Context(FragContainer fragments, bool keepMatches = false)
        {
            if (fragments != null)
            {
                scope = fragments;

                if (keepMatches == true)
                {
                    matches = scope.matches;
                }
                else
                {
                    matches = scope.cards.GetRange(0, fragments.cards.Count);
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
                    parentScope = actLogic.parent.fragments;
                }
            }
        }

        public Context(CardViz cardViz, bool keepMatches = false) : this(cardViz.fragments, keepMatches)
        {
            if (cardViz != null)
            {
                thisCard = cardViz;
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
                cardViz.Destroy();
            }
        }

        public void Destroy(CardViz cardViz)
        {
            if (cardViz != null)
            {
                toDestroy.Add(cardViz);
            }
        }

        public void ResetMatches()
        {
            matches = scope.cards.GetRange(0, actLogic.fragments.cards.Count);
        }

        public void SaveMatches()
        {
            scope.matches = matches;
        }

        public int Count(Fragment frag, int level)
        {
            if (frag != null)
            {
                if (frag == GameManager.Instance.thisAspect)
                {
                    return level * scope.Count(thisAspect);
                }
                else if (frag == GameManager.Instance.thisCard)
                {
                    return level * scope.Count(thisCard.card);
                }
                else
                {
                    return level * scope.Count(frag);
                }
            }
            else
            {
                return level;
            }
        }

        public Fragment ResolveFragment(Fragment frag)
        {
            if (frag != null)
            {
                if (frag == GameManager.Instance.thisAspect)
                {
                    return thisAspect;
                }
                else if (frag == GameManager.Instance.thisCard)
                {
                    return thisCard.card;
                }
                else
                {
                    return frag;
                }
            }
            else
            {
                return null;
            }
        }

        public Target ResolveTarget(Fragment frag)
        {
            if (frag != null)
            {
                if (frag == GameManager.Instance.thisAspect)
                {
                    return new Target(thisAspect);
                }
                else if (frag == GameManager.Instance.thisCard)
                {
                    return new Target(thisCard);
                }
                else if (frag == GameManager.Instance.matchedCards)
                {
                    return new Target(matches);
                }
                else
                {
                    return new Target(frag);
                }
            }
            else
            {
                return null;
            }
        }

        public List<CardViz> ResolveTargetCards(Target target, FragContainer scope)
        {
            if (target != null)
            {
                if (target.cards != null)
                {
                    return target.cards;
                }
                else if (target.fragment is Card)
                {
                   return scope.FindAll((Card)target.fragment);
                }
                else if (target.fragment is Aspect)
                {
                    return scope.FindAll((Aspect)target.fragment);
                }
            }
            return null;
        }
    }
}
