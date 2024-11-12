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
        [Tooltip("All of the AND Rules must pass. Modifiers are not applied.")]
        public List<Rule> and;
        [Tooltip("One of the OR Rules must pass. Modifiers are not applied.")]
        public List<Rule> or;
        [Space(10)]

        [Header("Modfiers")]
        public List<ActModifier> actModifiers;
        public List<CardModifier> cardModifiers;
        public List<TableModifier> tableModifiers;
        public List<PathModifier> pathModifiers;
        public List<DeckModifier> deckModifiers;

        [Header("Furthermore")]
        public List<Rule> furthermore;


        public static bool Evaluate(Context context, List<Test> tests, List<Rule> and, List<Rule> or)
        {
            foreach (var test in tests)
            {
                var r = test.Attempt(context);
                if (test.canFail == false && r == false)
                {
                    return false;
                }
            }

            foreach (var rule in and)
            {
                if (rule != null)
                {
                    using (var context2 = new Context(context))
                    {
                        var result = rule.Evaluate(context2);
                        if (result == false)
                        {
                            return false;
                        }
                    }
                }
            }

            foreach (var rule in or)
            {
                if (rule != null)
                {
                    using (var context2 = new Context(context))
                    {
                        var result = rule.Evaluate(context2);
                        if (result == true)
                        {
                            return true;
                        }
                    }
                }
            }

            return or.Count == 0;
        }

        public static void Execute(Context context, List<ActModifier> actMods,
                                    List<CardModifier> cardMods, List<TableModifier> tableMods,
                                    List<PathModifier> pathMods, List<DeckModifier> deckMods,
                                    List<Rule> furthermore)
        {
            if (context != null)
            {
                foreach (var actMod in actMods)
                {
                    context.actModifiers.Add(actMod.Evaluate(context));
                }
                foreach (var cardMod in cardMods)
                {
                    context.cardModifiers.Add(cardMod.Evaluate(context));
                }
                foreach (var tableMod in tableMods)
                {
                    context.tableModifiers.Add(tableMod.Evaluate(context));
                }
                foreach (var pathMod in pathMods)
                {
                    context.pathModifiers.Add(pathMod.Evaluate(context));
                }
                foreach (var deckMod in deckMods)
                {
                    context.deckModifiers.Add(deckMod.Evaluate(context));
                }

                foreach (var rule in furthermore)
                {
                    if (rule != null)
                    {
                        using (var context2 = new Context(context))
                        {
                            rule.Run(context2);
                        }
                    }
                }
            }
            context.ResetMatches();
        }

        public bool Evaluate(Context context) => Evaluate(context, tests, and, or);

        public void Execute(Context context) => Execute(context, actModifiers,
                                                        cardModifiers, tableModifiers,
                                                        pathModifiers, deckModifiers,
                                                        furthermore);

        public bool Run(Context context)
        {
            if (context != null)
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
                return false;
            }
        }
    }
}
