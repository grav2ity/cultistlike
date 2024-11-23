using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public class Target
    {
        public Fragment fragment;
        public List<CardViz> cards;


        public Target(Fragment frag)
        {
            fragment = frag;
        }

        public Target(CardViz cardViz)
        {
            cards = new List<CardViz>();
            cards.Add(cardViz);
        }

        public Target(List<CardViz> cards)
        {
            this.cards = cards.GetRange(0, cards.Count);
        }
    }


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
        public Fragment target;
        public Fragment fragment;
        public int level;
        public Fragment reference;


        public CardModifierC Evaluate(Context context)
        {
            var result = new CardModifierC();
            if (context != null && context.scope != null)
            {
                result.op = op;

                var tar = context.ResolveTarget(target);
                result.targetCards = context.ResolveTargetCards(tar, context.scope);
                result.what = context.ResolveTarget(fragment);
                result.level = context.Count(reference, level);
            }
            return result;
        }
    }

    public struct CardModifierC
    {
        public CardOp op;
        public List<CardViz> targetCards;
        public Target what;
        public int level;


        public void Execute(Context context)
        {
            if (context?.scope != null && targetCards != null && what != null)
            {
                switch (op)
                {
                    case CardOp.FragmentAdditive:
                        foreach (var targetCard in targetCards)
                        {
                            targetCard.fragments.Adjust(what, level);
                        }
                        break;
                    case CardOp.Transform:
                        if (what.fragment is Card)
                        {
                            if (level > 0)
                            {
                                foreach (var targetCard in targetCards)
                                {
                                    targetCard.Transform((Card)what.fragment);
                                    context.scope.Adjust(targetCard, level - 1);
                                }
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
        Adjust = 0,
        Grab = 20,
        // TransferToParent = 30,
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
                result.target = context.ResolveTarget(fragment);
                result.level = context.Count(reference, level);
            }
            return result;
        }
    }

    public struct ActModifierC
    {
        public ActOp op;
        public Target target;
        public int level;


        public void Execute(Context context)
        {
            if (context?.scope != null && target != null)
            {
                switch (op)
                {
                    case ActOp.Adjust:
                        Debug.Log("ADJUST");
                        if (target.cards != null)
                        {
                            foreach (var cardViz in target.cards)
                            {
                                Debug.Log("CARD");
                                var count = context.scope.Adjust(cardViz, level);
                                if (level < 0 && count < 0)
                                {
                                    context.Destroy(cardViz);
                                }
                            }
                        }
                        else if (target.fragment is Card && level < 0)
                        {
                            var cards = context.scope.FindAll((Card)target.fragment);
                            var count = context.scope.Adjust(target.fragment, level);
                            for (int i=0; i<count && i<cards.Count; i++)
                            {
                                context.Destroy(cards[i]);
                            }
                        }
                        else
                        {
                            context.scope.Adjust(target.fragment, level);
                        }
                        break;
                    case ActOp.Grab:
                        var targetCards = context.ResolveTargetCards(target, GameManager.Instance.fragments);
                        if (targetCards != null)
                        {
                            for (int i=0; i<level && i<targetCards.Count; i++)
                            {
                                if (targetCards[i].gameObject.activeSelf == true)
                                {
                                    context.scope.Add(targetCards[i]);
                                    context.actLogic.tokenViz.Grab(targetCards[i]);
                                }
                            }
                        }
                        break;
                    // case ActOp.TransferToParent:
                        // break;
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
                        context.actLogic.AddNextAct(act);
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
        // DrawNext = 10,
        // DrawPrevious = 20,
    }

    [Serializable]
    public struct DeckModifier
    {
        public DeckOp op;
        public Deck deck;
        // public Fragment reference;

        public DeckModifierC Evaluate(Context context)
        {
            var result = new DeckModifierC();
            if (context != null && context.scope != null)
            {
                result.op = op;
                result.deck = deck;
                // result.reference = context.Resolve(reference);
            }
            return result;
        }
    }

    [Serializable]
    public struct DeckModifierC
    {
        public DeckOp op;
        public Deck deck;
        public HeldFragment reference;

        public void Execute(Context context)
        {
            if (context != null && context.actLogic != null && context.scope != null && deck != null)
            {
                switch(op)
                {
                    case DeckOp.Draw:
                        var frag = deck.Draw();
                        if (frag is Card)
                        {
                            var newCardViz = GameManager.Instance.CreateCard((Card)frag);
                            newCardViz.ShowBack();

                            context.scope.Add(newCardViz);
                        }
                        break;
                    // case DeckOp.DrawNext:
                    //     {
                    //         if (reference != null)
                    //         {
                    //             var refer = reference.cardViz != null ? reference.cardViz.card : reference.fragment;
                    //             context.scope.Add(deck.DrawOffset(refer, 1));
                    //         }
                    //         break;
                    //     }
                    // case DeckOp.DrawPrevious:
                    //     {
                    //         if (reference != null)
                    //         {
                    //             var refer = reference.cardViz != null ? reference.cardViz.card : reference.fragment;
                    //             context.scope.Add(deck.DrawOffset(refer, -1));
                    //         }
                    //         break;
                    //     }
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
