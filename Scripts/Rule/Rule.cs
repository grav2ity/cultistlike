using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Rule")]
    public class Rule : ScriptableObject
    {
        [Header("Tests")]
        public bool invert;
        [Tooltip("All the tests must pass for this Rule to pass. Matched Cards from Card tests will be available in the Modifiers section.")]
        public List<Test> tests;
        [Tooltip("All of the And Rules must pass for this Rule to pass. Modifiers are not applied. Matched Cards are not carried in nor out.")]
        public List<Rule> and;
        [Tooltip("One of the Or Rules must pass for this Rule to pass. Modifiers are not applied. Matched Cards are not carried in nor out.")]
        public List<Rule> or;
        [Space(10)]

        [Header("Modfiers")]
        public List<ActModifier> actModifiers;
        public List<CardModifier> cardModifiers;
        public List<TableModifier> tableModifiers;
        public List<PathModifier> pathModifiers;
        public List<DeckModifier> deckModifiers;

        [Header("Furthermore")]
        [Tooltip("Additional Rules to be run if this Rule passed. Matched Cards are not carried in nor out.")]
        public List<Rule> furthermore;

        [Space(10)]
        [TextArea(3, 10)] public string text;


        public static bool Evaluate(Context context, List<Test> tests, List<Rule> and, List<Rule> or, bool invert, bool force = false)
        {
            if (invert == true && force == false)
            {
                return !Evaluate(context, tests, and, or, false);
            }

            foreach (var test in tests)
            {
                var r = test.Attempt(context);
                if (force == false && test.canFail == false && r == false)
                {
                    return false;
                }
            }

            //force is used only to save CardMatches for forced and spawned Acts
            if (force == true)
            {
                return true;
            }

            foreach (var rule in and)
            {
                if (rule != null)
                {
                    using (var context2 = new Context(context))
                    {
                        if (rule.Evaluate(context2) == false)
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
                        if (rule.Evaluate(context2) == true)
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
                        context.ResetMatches();
                        rule.Run(context);
                    }
                }
            }
            context.ResetMatches();
        }

        public bool Evaluate(Context context) => Evaluate(context, tests, and, or, invert);

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
