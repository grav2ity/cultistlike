using System;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;


namespace CultistLike
{
    public enum ReqOp
    {
        MoreOrEqual = 0,
        Equal = 1,
        LessOrEqual = 2,
        More = 3,
        NotEqual = 4,
        Less = 5,
        RandomChallenge = 10,
        RandomClash = 20
    }

    public enum ReqLoc
    {
        Scope          = 0,
        MatchedCards   = 1 << 5,
        Slots          = 1 << 2,
        Table          = 1 << 4,
        Heap           = 1 << 3,
        Free           = 1 << 7,
        Anywhere       = 1 << 6,
    }

    [Serializable]
    public class Test
    {
        public bool cardTest;
        public bool canFail;
        public ReqLoc loc1;
        public Fragment fragment1;
        [Space(10)]
        public ReqOp op;
        [Space(10)]
        [Tooltip("Fragment2 not set - value. Fragment2 set - multiplier. Accepts negative values.")]
        public int constant;
        public ReqLoc loc2;
        public Fragment fragment2;


        public bool Attempt(Context context)
        {
            int right;
            Fragment fragment1r = context.ResolveFragment(fragment1);
            Fragment fragment2r = context.ResolveFragment(fragment2);

            if (fragment2r == null)
            {
                right = constant;
            }
            else
            {
                right = constant * GetCount(context, loc2, fragment2r);
            }

            if (cardTest == true)
            {
                var scope = context.ResolveScope(loc1);

                List<CardViz> cards;
                if (loc1 == ReqLoc.MatchedCards)
                {
                    cards = context.matches;
                }
                else
                {
                    cards = scope.cards;
                }

                bool passed = false;
                if (fragment1r == null)
                {
                    if (right > 0)
                    {
                        right = Math.Min(right, context.matches.Count);
                        passed = true;
                        context.matches = context.matches.GetRange(0, right);
                    }
                }
                else
                {
                    List<CardViz> newMatches = new List<CardViz>();

                    if (fragment1r is Aspect)
                    {
                        var aspect = (Aspect)fragment1r;
                        int left;
                        foreach (var cardViz in cards)
                        {
                            bool result = false;
                            left = cardViz.fragTree.Count(aspect);

                            result = Compare(op, constant, left, right);
                            if (result == true && (loc1 != ReqLoc.Free || cardViz.free == true))
                            {
                                newMatches.Add(cardViz);
                                passed = true;
                            }
                        }
                    }
                    else if (fragment1r is Card)
                    {
                        var card = (Card)fragment1r;
                        foreach (var cardViz in cards)
                        {
                            bool result = cardViz.card == card;
                            if (result == true && (loc1 != ReqLoc.Free || cardViz.free == true))
                            {
                                newMatches.Add(cardViz);
                            }
                        }

                        int left = newMatches.Count;
                        passed = Compare(op, constant, left, right);
                    }

                    context.matches.Clear();
                    context.matches.InsertRange(0, newMatches);
                }

                return passed;
            }
            else
            {
                if (fragment1r != null)
                {
                    int left = GetCount(context, loc1, fragment1r);
                    return Compare(op, constant, left, right);
                }
                else
                {
                    return true;
                }
            }
        }

        public static bool Compare(ReqOp op, int constant, int left, int right)
        {
            switch (op)
            {
                case ReqOp.Equal:
                    return left == right;
                case ReqOp.NotEqual:
                    return left != right;
                case ReqOp.Less:
                    return left < right;
                case ReqOp.LessOrEqual:
                    return left <= right;
                case ReqOp.More:
                    return left > right;
                case ReqOp.MoreOrEqual:
                    return left >= right;
                case ReqOp.RandomChallenge:
                    return constant * left > Random.Range(0, 100);
                case ReqOp.RandomClash:
                    float div = (float)(left + right);
                    if (div > 0f)
                    {
                        float chance = ((float)left / div);
                        return chance > Random.Range(0f, 1f);
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }

        public static int GetCount(Context context, ReqLoc loc, Fragment fragment)
        {
            int total = 0;

            if (loc == ReqLoc.MatchedCards)
            {
                List<CardViz> cards = context.matches;

                if (fragment == null) //just count cards
                {
                    total = cards.Count;
                }
                else if (fragment is Aspect)
                {
                    var aspect = (Aspect)fragment;
                    HeldFragment ha = null;
                    foreach (var card in cards)
                    {
                        ha = card.fragTree.Find(aspect);
                        if (ha != null)
                        {
                            total += ha.count;
                            ha = null;
                        }
                    }
                }
                else if (fragment is Card)
                {
                    var car = (Card)fragment;
                    foreach (var card in cards)
                    {
                        if (card.card == car)
                        {
                            total += 1;
                        }
                    }
                }
            }
            else
            {
                var scope = context.ResolveScope(loc);

                total = scope.Count(fragment, loc == ReqLoc.Free);
            }

            return total;
        }
    }
}
