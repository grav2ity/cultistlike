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
            cards = new List<CardViz> { cardViz };
        }

        public Target(List<CardViz> cards)
        {
            this.cards = cards.GetRange(0, cards.Count);
        }
    }


    public enum CardOp
    {
        FragmentAdditive = 0,
        FragmentSet = 5,
        Transform = 10,
        Decay = 100,
        SetMemory = 140,
        // MoveToHeap = 170,
        // MoveFromHeap = 171,
        // Slot = 160,
        Spread = 150,
    }

    [Serializable]
    public struct CardModifier
    {
        public CardOp op;
        public Fragment target;
        public Fragment fragment;
        public int level;
        // public ReqLoc refLoc;
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
            if (context?.scope != null && targetCards != null)
            {
                for (int i=targetCards.Count-1; i>=0; i--)
                {
                    if (targetCards[i] == null)
                    {
                        targetCards.RemoveAt(i);
                        Debug.LogWarning("One of the target cards was destroyed. Please refer to the __MatchedCards WARNING section in the manual.");
                    }
                }

                switch (op)
                {
                    case CardOp.FragmentAdditive:
                        foreach (var targetCard in targetCards)
                        {
                            targetCard.fragTree.Adjust(what, level);
                        }
                        break;
                    case CardOp.FragmentSet:
                        foreach (var targetCard in targetCards)
                        {
                            int count = targetCard.fragTree.Count(what);
                            targetCard.fragTree.Adjust(what, level - count);
                        }
                        break;
                    case CardOp.Transform:
                        if (level > 0 && what != null)
                        {
                            foreach (var targetCard in targetCards)
                            {
                                if (what.fragment is Card card)
                                {
                                    targetCard.Transform(card);
                                    context.scope.Adjust(targetCard, level - 1);
                                }
                                else if (what.cards != null)
                                {
                                    foreach (var cardViz in what.cards)
                                    {
                                        targetCard.Transform(cardViz.card);
                                        context.scope.Adjust(targetCard, level - 1);
                                    }
                                }
                            }
                        }
                        break;
                    case CardOp.Decay:
                        foreach (var targetCard in targetCards)
                        {
                            if (what != null && what.fragment is Card card)
                            {
                                targetCard.Decay(card, level);
                            }
                            else
                            {
                                targetCard.Decay(targetCard.card.decayTo, targetCard.card.lifetime);
                            }
                        }
                        break;
                    case CardOp.SetMemory:
                        //TODO only level times?
                        foreach (var targetCard in targetCards)
                        {
                            if (what != null)
                            {
                                if (what.fragment is Aspect)
                                {
                                    targetCard.fragTree.memoryFragment = what.fragment;
                                }
                                else if (what.cards != null && what.cards.Count > 0)
                                {
                                    targetCard.fragTree.memoryFragment = what.cards[0].fragTree.memoryFragment;
                                }
                            }
                            else
                            {
                                targetCard.fragTree.memoryFragment = null;
                            }
                        }
                        break;
                    case CardOp.Spread:
                        if (what != null)
                        {
                            foreach (var targetCard in targetCards)
                            {
                                if (what.fragment is Card)
                                {
                                    // targetCard.Transform((Card)what.fragment);
                                    // context.scope.Adjust(targetCard, level - 1);
                                }
                                else if (what.cards != null)
                                {
                                    foreach (var cardViz in what.cards)
                                    {
                                        if (targetCard.fragTree.cards.FindAll(x => x.MemoryEqual(cardViz)).Count == 0)
                                        {
                                            var newCardViz = targetCard.fragTree.Adjust(cardViz, 1);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }
    }


    public enum ActOp
    {
        Adjust = 0,
        Grab = 20,
        Expulse = 30,
        SetMemory = 40,
        RunTriggers = 50,
    }

    [Serializable]
    public struct ActModifier
    {
        public ActOp op;
        public Fragment fragment;
        [Tooltip("Reference not set - value. Reference set - multiplier. Accepts negative values.")]
        public int level;
        public ReqLoc refLoc;
        public Fragment reference;


        public ActModifierC Evaluate(Context context)
        {
            var result = new ActModifierC();

            if (context != null && context.scope != null)
            {
                result.op = op;
                result.target = context.ResolveTarget(fragment);
                var frag = context.ResolveFragment(reference);
                if (frag != null)
                {
                    result.level = level * Test.GetCount(context, refLoc, frag);
                }
                else
                {
                    result.level = level;
                }
                //only for Grab
                result.all = level == 0;
            }
            return result;
        }
    }

    public struct ActModifierC
    {
        public ActOp op;
        public Target target;
        public int level;
        public bool all;


        public void Execute(Context context)
        {
            if (context?.scope != null && target != null)
            {
                switch (op)
                {
                    case ActOp.Adjust:
                        if (target.cards != null)
                        {
                            foreach (var cardViz in target.cards)
                            {
                                var count = context.scope.Adjust(cardViz, level);
                                if (level < 0 && count < 0)
                                {
                                    context.Destroy(cardViz);
                                }
                            }
                        }
                        else if (target.fragment is Card card && level < 0)
                        {
                            var cards = context.scope.FindAll(card);
                            var count = context.scope.Adjust(target.fragment, level);
                            if (count < 0)
                            {
                                count = -count;
                                for (int i=0; i<count && i<cards.Count; i++)
                                {
                                    context.Destroy(cards[i]);
                                }
                            }
                        }
                        else
                        {
                            context.scope.Adjust(target.fragment, level);
                        }
                        break;
                    case ActOp.Grab:
                        var targetCardsY = context.ResolveTargetCards(target, GameManager.Instance.root);
                        if (targetCardsY != null)
                        {
                            var targetCards = new List<CardViz>();
                            foreach (var cardViz in targetCardsY)
                            {
                                var targetCard = cardViz.stack != null ? cardViz.stack : cardViz;
                                if (targetCard.gameObject.activeSelf)
                                {
                                    targetCards.Add(targetCard);
                                }
                            }

                            level = all ? targetCards.Count : level;
                            for (int i=0; i<level && i<targetCards.Count; i++)
                            {
                                context.actLogic.tokenViz.Grab(targetCards[i]);
                            }
                        }
                        break;
                    case ActOp.Expulse:
                        targetCardsY = context.ResolveTargetCards(target, GameManager.Instance.root);
                        if (targetCardsY != null)
                        {
                            foreach (var cardViz in targetCardsY)
                            {
                                cardViz.free = true;
                                cardViz.interactive = true;
                                cardViz.Show();
                                cardViz.transform.position = cardViz.Position(true);
                                GameManager.Instance.table.ReturnToTable(cardViz);
                            }
                        }
                        break;
                    case ActOp.SetMemory:
                        if (target.fragment != null)
                        {
                            context.scope.memoryFragment = target.fragment;
                        }
                        else if (target.cards.Count > 0)
                        {
                            context.scope.memoryFragment = target.cards[0].fragTree.memoryFragment;
                        }
                        else
                        {
                            context.scope.memoryFragment = null;
                        }
                        break;
                    case ActOp.RunTriggers:
                        context.actLogic?.InjectTriggers(target);
                        break;
                }
            }
        }
    }


    public enum PathOp
    {
        BranchOut = 0,
        // InjectNextAct = 10,
        // InjectAltAct = 11,
        ForceAct = 20,
        SetCallback = 40,
        Callback = 41,
        GameOver = 80,
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
                    case PathOp.BranchOut:
                        context.actLogic.BranchOut(act);
                        break;
                    // case PathOp.InjectNextAct:
                    //     context.actLogic.InjectNextAct(act);
                    //     break;
                    // case PathOp.InjectAltAct:
                    //     context.actLogic.InjectAltAct(act);
                    //     break;
                    case PathOp.ForceAct:
                        context.actLogic.SetForceAct(act);
                        break;
                    case PathOp.SetCallback:
                        context.actLogic.SetCallback(act);
                        break;
                    case PathOp.Callback:
                        context.actLogic.DoCallback();
                        break;
                    case PathOp.GameOver:
                        GameManager.Instance.Reset();
                        GameManager.Instance.SpawnAct(act, null, null);
                        break;
                }
            }
        }
    }


    public enum DeckOp
    {
        Draw = 0,
        DrawNext = 10,
        DrawPrevious = 20,
        Add = 50,
        AddFront = 51,
        ForwardShift = 100
    }

    [Serializable]
    public struct DeckModifier
    {
        public DeckOp op;
        public Deck deck;
        public Fragment deckFrom;
        public Fragment fragment;

        public DeckModifierC Evaluate(Context context)
        {
            var result = new DeckModifierC();
            if (context != null)
            {
                result.op = op;
                if (deck == null && deckFrom != null)
                {
                    var frag = context.ResolveFragment(deckFrom);
                    if (frag == GameManager.Instance.matchedCards &&
                        context.matches != null && context.matches.Count > 0)
                    {
                        frag = context.matches[0].card;
                    }
                    result.deck = frag.deck;
                }
                else
                {
                    result.deck = deck;
                }
                result.target = context.ResolveTarget(fragment);
            }
            return result;
        }
    }

    [Serializable]
    public struct DeckModifierC
    {
        public DeckOp op;
        public Deck deck;
        public Target target;

        public void Execute(Context context)
        {
            if (context != null && context.scope != null && deck != null)
            {
                if (op == DeckOp.Draw)
                {
                    //TODO
                    int maxTries = 3;
                    Fragment frag;
                    do
                    {
                        frag = deck.Draw();
                        maxTries--;
                    }
                    while (maxTries > 0 && GameManager.Instance.AllowedToCreate((Card)frag) == false);

                    CreateCard(context, deck, frag);
                }
                else if (target != null)
                {
                    if (target.fragment != null)
                    {
                        switch(op)
                        {
                            case DeckOp.DrawNext:
                                CreateCard(context, deck, deck.DrawOffset(target.fragment, 1));
                                break;
                            case DeckOp.DrawPrevious:
                                CreateCard(context, deck, deck.DrawOffset(target.fragment, -1));
                                break;
                            case DeckOp.Add:
                                deck.Add(target.fragment);
                                break;
                            case DeckOp.AddFront:
                                deck.AddFront(target.fragment);
                                break;
                            case DeckOp.ForwardShift:
                                var targetCards = context.ResolveTargetCards(target, context.scope);
                                foreach (var cardViz in targetCards)
                                {
                                    ShiftCard(cardViz, deck.DrawOffset(cardViz.card, 1));
                                }
                                break;
                        }
                    }
                    else if (target.cards != null)
                    {
                        foreach (var cardViz in target.cards)
                        {
                            switch(op)
                            {
                                case DeckOp.DrawNext:
                                    CreateCard(context, deck, deck.DrawOffset(cardViz.card, 1));
                                    break;
                                case DeckOp.DrawPrevious:
                                    CreateCard(context, deck, deck.DrawOffset(cardViz.card, -1));
                                    break;
                                case DeckOp.Add:
                                    deck.Add(cardViz.card);
                                    break;
                                case DeckOp.AddFront:
                                    deck.AddFront(cardViz.card);
                                    break;
                                case DeckOp.ForwardShift:
                                    ShiftCard(cardViz, deck.DrawOffset(cardViz.card, 1));
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void CreateCard(Context context, Deck deck, Fragment drawnFrag)
        {
            if (context != null && drawnFrag is Card card)
            {
                var newCardViz = context.scope.Add(card);
                newCardViz.ShowBack();

                foreach (var frag in deck.tagOn)
                {
                    if (frag != null)
                    {
                        newCardViz.fragTree.Add(frag);
                    }
                }
                if (deck.memoryFragment != null)
                {
                    newCardViz.fragTree.memoryFragment = deck.memoryFragment;
                }
            }
        }

        private void ShiftCard(CardViz cardViz, Fragment drawnFrag)
        {
            if (cardViz != null && drawnFrag is Card card)
            {
                cardViz.Transform(card);
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
                        GameManager.Instance.SpawnAct(act, context.scope, context.actLogic.tokenViz);
                        break;
                    case TableOp.SpawnToken:
                        GameManager.Instance.SpawnToken(act.token, context.scope, context.actLogic.tokenViz);
                        break;
                }
            }
        }
    }
}
