using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Rule")]
    public class Rule : ScriptableObject
    {
        [Header("Tests")]
        public List<Test> tests;
        [Tooltip("One of the Rules must pass. Modifiers are not applied.")]
        public List<Rule> orRules;
        [Space(10)]

        [Header("Modfiers")]
        public List<ActModifier> actModifiers;
        public List<CardModifier> cardModifiers;
        public List<TableModifier> tableModifiers;
        public List<PathModifier> pathModifiers;
        public List<DeckModifier> deckModifiers;
        [Tooltip("All modifiers are applied. Tests are ignored.")]
        public List<Rule> modRules;


        public bool Evaluate(Context context)
        {
            foreach (var test in tests)
            {
                var r = test.Attempt(context);
                if (test.canFail == false && r == false)
                {
                    return false;
                }
            }

            // foreach (var rule in orRules)
            // {
            //     context.ResetMatches();
            //     if (rule.Evaluate(context) == true)
            //     {
            //         return true;
            //     }
            // }

            if (orRules.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Execute(Context context)
        {
            if (context != null)
            {
                foreach (var actModifier in actModifiers)
                {
                    context.actModifiers.Add(actModifier.Evaluate(context));
                }
                foreach (var cardModifier in cardModifiers)
                {
                    context.cardModifiers.Add(cardModifier.Evaluate(context));
                }
                foreach (var tableModifier in tableModifiers)
                {
                    context.tableModifiers.Add(tableModifier.Evaluate(context));
                }
                foreach (var pathModifier in pathModifiers)
                {
                    context.pathModifiers.Add(pathModifier.Evaluate(context));
                }
                foreach (var deckModifier in deckModifiers)
                {
                    context.deckModifiers.Add(deckModifier.Evaluate(context));
                }

                foreach (var rule in modRules)
                {
                    rule?.Execute(context);
                }
            }
            context.ResetMatches();
        }

        public bool Run(Context context)
        {
            if (context != null && context.source != null && context.actLogic != null)
            {
                if (Evaluate(context) == false)
                {
                    return false;
                }
                else
                {
                    Execute(context);
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
