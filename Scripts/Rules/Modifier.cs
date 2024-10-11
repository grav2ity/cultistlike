using System;
using System.Collections.Generic;

using UnityEngine;

using DG.Tweening;


namespace CultistLike
{
    public enum NumOp
    {
        Set,
        Add,
        Multiply
    }

    [Serializable]
    public struct AspectModifier
    {
        public Aspect aspect;
        public NumOp op;
        public int x;

        public void Apply(AspectViz aspectViz)
        {
            if (aspectViz != null && aspectViz.aspect == aspect)
            {
                switch (op)
                {
                    case NumOp.Set:
                        aspectViz.count = x;
                        break;
                    case NumOp.Add:
                        aspectViz.count += x;
                        break;
                    case NumOp.Multiply:
                        aspectViz.count *= x;
                        break;
                    default:
                        break;
                }
            }
            return;
        }
    }

    public enum CardOp
    {
        Add
    }

    [Serializable]
    public struct CardModifier
    {
        public Card card;

        public void Apply(ref CardViz cardViz)
        {
            if (cardViz != null && cardViz.card == card )
            {
            }
            return;
        }
    }

    public enum ActOp
    {
        AddCard,
        RemoveCard,
        ConsumeExtraSlotCard
    }

    [Serializable]
    public struct ActModifier
    {
        public ActOp op;
        public Requirement cardReqs;

        public void Apply(ActLogic actLogic)
        {
            if (actLogic != null)
            {
                CardViz cardViz = null;
                switch (op)
                {
                    case ActOp.AddCard:
                        actLogic.HoldCard(cardReqs.card);
                        break;
                    case ActOp.RemoveCard:
                        cardViz = actLogic.UnholdCard(cardReqs);
                        GameManager.Instance.DestroyCard(cardViz);
                        break;
                    default:
                        break;
                }
            }
            return;
        }
    }

    public enum BoardOp
    {
        SpawnAct,
    }

    [Serializable]
    public struct TableModifier
    {
        public BoardOp op;
        public Act act;

        public void Apply(Viz viz)
        {
            switch (op)
            {
                case BoardOp.SpawnAct:
                    if (act != null && viz != null )
                    {
                        var newActViz = UnityEngine.Object.Instantiate(GameManager.Instance.actPrefab,
                                                    viz.transform.position, Quaternion.identity);
                        newActViz.SetAct(act);

                        var root = newActViz.transform;
                        var localScale = root.localScale;

                        GameManager.Instance.table.Place(viz, new List<Viz> { newActViz });

                        root.localScale = new Vector3(0f, 0f, 0f);
                        root.DOScale(localScale, 1);
                    }
                    break;
                default:
                    break;
            }
            return;
        }
    }
}
