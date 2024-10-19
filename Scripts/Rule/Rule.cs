using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public enum ReqOp
    {
        MoreOrEqual,
        Equal,
        LessOrEqual,
        More,
        NotEqual,
        Less,
    }

    public enum ReqLoc
    {
        Scope,
        Slots,
        Table,
        Anywhere,
        MatchedCards,
        CardInScope,
        CardInSlots,
        CardOnTable,
        CardAnywhere,
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
        public int constant;
        public ReqLoc loc2;
        public Fragment fragment2;

        public bool Attempt(FragContainer scope)
        {
            int right;
            if (fragment2 == null)
            {
                right = constant;
            }
            else
            {
                right = GetCount(scope, loc2, fragment2, ReqOp.Equal, 1, false);
            }

            int left = GetCount(scope, loc1, fragment1, op, right, true);

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
                default:
                    return false;
            }
        }

        public int GetCount(FragContainer scope, ReqLoc loc, Fragment fragment, ReqOp op, int val, bool updateMatches)
        {

            if (loc < ReqLoc.MatchedCards)
            {
                FragContainer location1 = null;
                switch (loc)
                {
                    case ReqLoc.Scope:
                        location1 = scope;
                        break;
                    default:
                        break;
                }
                if (location1 != null)
                {
                    return fragment.CountInContainer(location1);
                }
                else
                {
                    //ERROR
                    return 0;
                }
            }

            else if (loc >= ReqLoc.MatchedCards) //looking for cards
            {
                List<CardViz> location1 = null;
                List<CardViz> newMatches = new List<CardViz>();
                switch (loc)
                {
                    case ReqLoc.MatchedCards:
                        location1 = scope.matches;
                        break;
                    case ReqLoc.CardInScope:
                        location1 = scope.cards;
                        break;
                    default:
                        break;
                }

                Aspect aspect;
                if (fragment == null) //just count cards
                {
                    return scope.matches.Count;

                }
                else if (fragment is Aspect)
                {
                    aspect = (Aspect)fragment;
                    foreach (var card in location1)
                    {
                        HeldAspect ha = null;
                        switch (op)
                        {
                            case ReqOp.Equal:
                                ha = card.fragments.Find(aspect, x => x == val);
                                break;
                            case ReqOp.NotEqual:
                                ha = card.fragments.Find(aspect, x => x != val);
                                break;
                            case ReqOp.Less:
                                ha = card.fragments.Find(aspect, x => x < val);
                                break;
                            case ReqOp.LessOrEqual:
                                ha = card.fragments.Find(aspect, x => x <= val);
                                break;
                            case ReqOp.More:
                                ha = card.fragments.Find(aspect, x => x > val);
                                break;
                            case ReqOp.MoreOrEqual:
                                ha = card.fragments.Find(aspect, x => x >= val);
                                break;
                            default:
                                break;
                        }
                        if (ha != null)
                        {
                            newMatches.Add(card);
                            ha = null;
                        }
                    }
                }

                if (updateMatches == true)
                {
                    scope.matches = newMatches;
                }
                return newMatches.Count;
            }
            return 0;
        }
    }


    [CreateAssetMenu(menuName = "Rule")]
    public class Rule : ScriptableObject
    {
        public float time;
        [Space(10)]
        public List<Test> tests;
        [Space(10)]

        public List<ActModifier> actModifiers;
        public List<CardModifier> cardModifiers;
        public List<TableModifier> tableModifiers;


        public bool Attempt(FragContainer scope)
        {
            // scope.matches.Clear();
            scope.matches = scope.cards;

            foreach (var test in tests)
            {
                var r = test.Attempt(scope);
                if (test.canFail == false && r == false)
                {
                    return false;
                }
            }
            return true;
        }

        public bool Execute(ActLogic actLogic, bool reset = false)
        {
            if (reset == true)
            {
                actLogic.fragments.matches = actLogic.fragments.cards;
            }

            foreach (var test in tests)
            {
                var r = test.Attempt(actLogic.fragments);
                if (test.canFail == false && r == false)
                {
                    return false;
                }
            }

            // foreach (var actModifier in result.actModifiers)
            // {
            //     actModifier.Apply(actLogic);
            // }
            // // foreach (var tableModifier in result.tableModifiers)
            // // {
            // //     tableModifier.Apply(actLogic.actViz);
            // // }
            return true;
        }
    }
}
