using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{

    public enum CardOp
    {
        AddAspect,
        RemoveAspect
    }

    [Serializable]
    public struct CardModifier
    {
        public CardOp op;
        public Card card;
        public List<Fragment> fragments;

        public void Apply(ref CardViz cardViz)
        {
            //TODO
            if (cardViz != null && cardViz.card == card )
            {
            }
            return;
        }
    }

    public enum ActOp
    {
        Fragment,
        DestroyMatchedCard
    }

    [Serializable]
    public struct ActModifier
    {
        public ActOp op;
        public Fragment fragment;
        [Tooltip("Reference not set - value. Reference set - multiplier. Accepts negative values.")]
        public int level;
        public Fragment reference;


        public void Apply(ActLogic actLogic)
        {
            if (actLogic != null && fragment != null && level != 0)
            {
                level = reference ? level * actLogic.fragments.Count(reference) : level;
                switch (op)
                {
                    case ActOp.Fragment:
                        actLogic.AdjustFragment(fragment, level);
                        break;
                    case ActOp.DestroyMatchedCard:
                        if (actLogic.fragments.matches.Count > 0)
                        {
                            actLogic.DestroyCard(actLogic.fragments.matches[0]);
                        }
                        break;
                    default:
                        break;
                }
            }
            return;
        }
    }

    public enum TableOp
    {
        SpawnAct,
        SpawnToken
    }

    [Serializable]
    public class TableModifier
    {
        public TableOp op;
        public Act act;

        public void Apply(Viz viz)
        {
            switch (op)
            {
                case TableOp.SpawnAct:
                    GameManager.Instance.SpawnAct(act, viz);
                    break;
                case TableOp.SpawnToken:
                    GameManager.Instance.SpawnToken(act.token, viz);
                    break;
                default:
                    break;
            }
            return;
        }
    }
}
