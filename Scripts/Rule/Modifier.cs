﻿using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public enum CardOp
    {
        FragmentAdditive = 0,
        Transform = 10,
        Decay = 100,
    }

    [Serializable]
    public struct CardModifier
    {
        public CardOp op;
        public Fragment onto;
        public Fragment what;
        public int level;
        public Fragment reference;
        //TODO probability


        public CardModifierC Evaluate(Context context)
        {
            var result = new CardModifierC();
            if (context != null && context.scope != null)
            {
                result.op = op;

                result.onto = context.Resolve(onto);
                result.what = context.Resolve(what);
                result.reference = context.Resolve(reference);
                result.level = result.reference != null ? level * context.scope.Count(result.reference) : level;
            }
            return result;
        }
    }

    public struct CardModifierC
    {
        public CardOp op;
        public HeldFragment onto;
        public HeldFragment what;
        public int level;
        public HeldFragment reference;


        public void Execute(Context context)
        {
            if (context != null && context.scope != null && onto != null && what != null)
            {
                List<CardViz> targetCards;
                if (onto.cardViz != null)
                {
                    targetCards = new List<CardViz>();
                    targetCards.Add(onto.cardViz);
                }
                else if (onto.fragment is Card)
                {
                    targetCards = context.scope.FindAll((Card)onto.fragment);
                }
                else if (onto.fragment is Aspect)
                {
                    targetCards = context.scope.FindAll((Aspect)onto.fragment);
                }
                else
                {
                    return;
                }

                switch (op)
                {
                    case CardOp.FragmentAdditive:
                        foreach (var targetCard in targetCards)
                        {
                            //TODO TEST THIS
                            targetCard.fragments.Adjust(what, level);
                        }
                        break;
                    case CardOp.Transform:
                        if (what.fragment is Card)
                        {
                            foreach (var targetCard in targetCards)
                            {
                                targetCard.Transform((Card)what.fragment);
                            }
                        }
                        break;
                    case CardOp.Decay:
                        foreach (var targetCard in targetCards)
                        {
                            if (what.fragment is Card)
                            {
                                targetCard.Decay((Card)what.fragment, level);
                            }
                            else
                            {
                                targetCard.Decay(targetCard.card.decayTo, targetCard.card.lifetime);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return;
        }
    }


    public enum ActOp
    {
        Fragment = 0,
        TransferToParent = 30,
        Destroy = 100,
        Duplicate = 110
    }

    [Serializable]
    public struct ActModifier
    {
        public ActOp op;
        public Fragment fragment;
        [Tooltip("Reference not set - value. Reference set - multiplier. Accepts negative values.")]
        public int level;
        public Fragment reference;


        public ActModifierC Evaluate(Context context)
        {
            var result = new ActModifierC();

            if (context != null && context.scope != null)
            {
                result.op = op;
                result.fragment = context.Resolve(fragment);
                result.reference = context.Resolve(reference);
                result.level = result.reference != null ? level * context.scope.Count(result.reference) : level;
            }
            return result;
        }
    }

    public struct ActModifierC
    {
        public ActOp op;
        public HeldFragment fragment;
        [Tooltip("Reference not set - value. Reference set - multiplier. Accepts negative values.")]
        public int level;
        public HeldFragment reference;

        public void Execute(Context context)
        {
            if (context != null && context.scope != null && context.scope != null)
            {
                switch (op)
                {
                    case ActOp.Fragment:
                        context.scope.Adjust(fragment, level);
                        break;
                    case ActOp.TransferToParent:
                        if (context.parentScope != null)
                        {
                            //TODO update visuals
                            if (level > 0)
                            {
                                int count = context.scope.Adjust(fragment, -level);
                                context.parentScope.Adjust(fragment, -count);
                            }
                            else if (level < 0)
                            {
                                int count = context.parentScope.Adjust(fragment, level);
                                context.scope.Adjust(fragment, -count);
                            }
                        }
                        break;
                    case ActOp.Destroy:
                        context.scope.Remove(fragment);
                        context.Destroy(fragment);
                        break;
                    case ActOp.Duplicate:
                        //TODO seriously
                        // context.scope.Add(fragment);
                        break;
                    default:
                        break;
                }
            }
            return;
        }
    }

    public enum PathOp
    {
        NextAct = 0,
        AltAct = 10,
        ForceAct = 20,
    }

    [Serializable]
    public struct PathModifier
    {
        public PathOp op;
        public Act act;


        public PathModifier Evaluate(Context context)
        {
            return this;
        }

        public void Execute(Context context)
        {
            if (context != null && context.actLogic != null)
            {
                switch (op)
                {
                    case PathOp.NextAct:
                        context.actLogic.nextActs.Add(act);
                        break;
                    case PathOp.AltAct:
                        context.actLogic.altActs.Add(act);
                        break;
                    case PathOp.ForceAct:
                        context.actLogic.ForceAct(act);
                        break;
                    default:
                        break;
                }
            }
            return;
        }
    }

    public enum DeckOp
    {
        Draw = 0,
        DrawNext = 10,
        DrawPrevious = 20,
    }

    [Serializable]
    public struct DeckModifier
    {
        public DeckOp op;
        public Deck deck;
        public Fragment reference;


        public DeckModifier Evaluate(Context context)
        {
            return this;
        }

        public void Execute(Context context)
        {
            if (context != null && context.actLogic != null && context.scope != null && deck != null)
            {
                switch(op)
                {
                    case DeckOp.Draw:
                        context.scope.Add(deck.Draw());
                        break;
                    case DeckOp.DrawNext:
                        {
                            var refer = reference != null ? reference : context.matches[0].card;
                            context.scope.Add(deck.DrawOffset(refer, 1));
                            break;
                        }
                    case DeckOp.DrawPrevious:
                        {
                            var refer = reference != null ? reference : context.matches[0].card;
                            context.scope.Add(deck.DrawOffset(refer, -1));
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }

    public enum TableOp
    {
        SpawnAct = 0,
        SpawnToken = 10
    }

    [Serializable]
    public struct TableModifier
    {
        public TableOp op;
        public Act act;

        public TableModifier Evaluate(Context context)
        {
            return this;
        }

        public void Execute(Context context)
        {
            if (context != null && context.actLogic != null)
            {
                switch (op)
                {
                    case TableOp.SpawnAct:
                        var tokenViz = GameManager.Instance.SpawnAct(act, context.actLogic);
                        break;
                    case TableOp.SpawnToken:
                        GameManager.Instance.SpawnToken(act.token, context.actLogic.tokenViz);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
