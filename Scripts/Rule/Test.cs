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

    [Flags]
    public enum ReqLoc
    {
        Scope      = 0,
        Matched    = 1 << 5,
        Card       = 1 << 1,
        Cards      = 1 << 2,
        Parent     = 1 << 3,
        Table      = 1 << 4,
    }

    [Serializable]
    public class Test
    {
        [Tooltip("This Test can fail. Use this option to match Cards. You can reference matched Cards.")]
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


        public bool Attempt(Context context, ActLogic parent = null)
        {
            int min, max;
            int right;
            if (fragment2 == null)
            {
                right = constant;
            }
            else
            {
                right = constant * GetCount(out min, out max, context, loc2, fragment2, false);
            }

            int left = GetCount(out min, out max, context, loc1, fragment1, true);

            switch (op)
            {
                case ReqOp.Equal:
                    return left == right;
                case ReqOp.NotEqual:
                    return left != right;
                case ReqOp.Less:
                    return min < right;
                case ReqOp.LessOrEqual:
                    return min <= right;
                case ReqOp.More:
                    return max > right;
                case ReqOp.MoreOrEqual:
                    return max >= right;
                case ReqOp.RandomChallenge:
                    return constant * max > Random.Range(0, 100);
                case ReqOp.RandomClash:
                    float chance = ((float)max / (float)(max + right));
                    return chance > Random.Range(0f, 1f);
                default:
                    return false;
            }
        }


        public int GetCount(out int min, out int max, Context context, ReqLoc loc, Fragment fragment, bool updateMatches)
        {
            var scope = context.scope;
            var parent = context.parent;

            List<CardViz> newMatches = new List<CardViz>();
            min = 0;
            max = 0;
            int total = 0;

            if ((loc & ReqLoc.Parent) != 0)
            {
                if (parent != null)
                {
                    scope = parent.fragments;
                }
                else
                {
                    //ERROR
                    return 0;
                }
            }
            else if ((loc & ReqLoc.Table) != 0)
            {
                scope = GameManager.Instance.table.fragments;
            }

            List<CardViz> cards;
            if ((loc & ReqLoc.Matched) != 0)
            {
                cards = context.matches;
            }
            else
            {
                cards = scope.cards;
            }

            if (loc == ReqLoc.Scope || loc == ReqLoc.Table)
            {
                total = fragment.CountInContainer(scope);
            }
            else if ((loc & (ReqLoc.Card | ReqLoc.Cards)) != 0)
            {
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
                        ha = card.fragments.Find(aspect);
                        if (ha != null)
                        {
                            newMatches.Add(card);
                            total += ha.count;
                            max = Math.Max(max, ha.count);
                            min = Math.Min(min, ha.count);
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
                            newMatches.Add(card);
                            total +=1;
                        }
                    }
                    max = total;
                    min = total;
                }
                if (updateMatches == true)
                {
                    context.matches.Clear();
                    context.matches.InsertRange(0, newMatches);
                }
            }

            if ((loc & ReqLoc.Card) == 0)
            {
                max = total;
                min = total;
            }
            return total;
        }
    }
}
