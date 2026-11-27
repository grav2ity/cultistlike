using System.Linq;

using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;


namespace CultistLike
{
    [InitializeOnLoad]
    public static class PlayModeAssetDrop
    {
        static PlayModeAssetDrop()
        {
            DragAndDrop.AddDropHandler
            (
                HandleDrop
            );
        }

        static void CardDrag(Card card, Object target)
        {
            CardViz cardViz;
            TokenViz tokenViz;
            ActWindow actWindow;
            ArrayTable table;

            if ((cardViz = target.GetComponentInParent<CardViz>()) != null)
            {
                cardViz.Transform(card);
            }
            else if ((tokenViz = target.GetComponentInParent<TokenViz>()) != null)
            {
                var newCardViz = GameManager.Instance.CreateCard(card);
                if (newCardViz != null)
                    newCardViz.ParentTo(tokenViz.actWindow.transform, true);
            }
            else if ((actWindow = target.GetComponentInParent<ActWindow>()) != null)
            {
                var newCardViz = GameManager.Instance.CreateCard(card);
                if (newCardViz != null)
                    newCardViz.ParentTo(actWindow.transform, true);
            }
            else if ((table = target.GetComponentInParent<ArrayTable>()) != null)
            {
                table.ReturnToTable(GameManager.Instance.CreateCard(card));
            }
        }

        static void AspectDrag(Aspect aspect, Object target)
        {
            CardViz cardViz;
            TokenViz tokenViz;
            ActWindow actWindow;

            if ((cardViz = target.GetComponentInParent<CardViz>()) != null)
            {
                cardViz.fragTree.Add(aspect);
            }
            else if ((tokenViz = target.GetComponentInParent<TokenViz>()) != null)
            {
                tokenViz.actWindow.AddFragment(aspect);
            }
            else if ((actWindow = target.GetComponentInParent<ActWindow>()) != null)
            {
                actWindow.AddFragment(aspect);
            }
        }

        static void ActDrag(Act act, Object target)
        {
            TokenViz tokenViz;
            ActWindow actWindow;

            if ((tokenViz = target.GetComponentInParent<TokenViz>()) != null)
            {
                tokenViz.actWindow.ForceAct(act);
            }
            else if ((actWindow = target.GetComponentInParent<ActWindow>()) != null)
            {
                actWindow.ForceAct(act);
            }
            else if (target.GetComponentInParent<ArrayTable>() != null)
            {
                if (act.token != null)
                    GameManager.Instance.SpawnAct(act, null, null, true);
            }
        }

        static void RuleDrag(Rule rule, Object target)
        {
            CardViz cardViz;
            TokenViz tokenViz;
            ActWindow actWindow;

            if ((cardViz = target.GetComponentInParent<CardViz>()) != null)
            {
                using (var context = new Context(cardViz))
                {
                    rule.Run(context);
                }
            }
            else if ((tokenViz = target.GetComponentInParent<TokenViz>()) != null)
            {
                tokenViz.actWindow.ForceRule(rule);
            }
            else if ((actWindow = target.GetComponentInParent<ActWindow>()) != null)
            {
                actWindow.ForceRule(rule);
            }
        }

        static void TokenDrag(Token token, Object target)
        {
            ArrayTable table;

            if ((table = target.GetComponentInParent<ArrayTable>()) != null)
            {
                GameManager.Instance.SpawnToken(token, null, null);
            }
        }


        static DragAndDropVisualMode HandleDrop(
            Object dropUpon,
            Vector3 position,
            Vector2 mousePosition,
            Transform parent,
            bool perform
        )
        {
            if (!EditorApplication.isPlaying)
                return DragAndDropVisualMode.None;

            if (dropUpon == null)
                return DragAndDropVisualMode.None;

            if (DragAndDrop.objectReferences.Count() == 0)
                return DragAndDropVisualMode.None;

            if (!perform)
                return DragAndDropVisualMode.Copy;

            GameObject target = HandleUtility.PickGameObject(
                mousePosition,
                false
            );

            if (target == null)
                return DragAndDropVisualMode.Copy;

            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is Card card)
                    CardDrag(card, target);
                else if (obj is Aspect aspect)
                    AspectDrag(aspect, target);
                else if (obj is Act act)
                    ActDrag(act, target);
                else if (obj is Token token)
                    TokenDrag(token, target);
                else if (obj is Rule rule)
                    RuleDrag(rule, target);
            }

            return DragAndDropVisualMode.Copy;
        }
    }
}