using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Act")]
    public class Act : ScriptableObject
    {
        public string label;
        [Tooltip("Limits execution to this Token. Mandatory for initial Acts or Acts that will be spawned.")]
        public Token token;
        public bool initial;
        [Space(10)]
        public float time;

        [Header("Entry Tests")]
        [Tooltip("All Tests must pass to enter this Act.")]
        public List<Test> tests;
        [Tooltip("All Rules must pass to enter this Act. Card matches do not carry from the Tests above nor between Rules.")]
        public List<Rule> testRules;

        [Header("On Enter")]
        [Tooltip("All Modifiers from all the Rules will be applied. Tests are ignored.")]
        public List<Rule> onEnterRules;

        [Header("Fragments")]
        public List<Fragment> fragments;

        [Header("On Complete")]
        public List<ActModifier> actModifiers;
        public List<CardModifier> cardModifiers;
        public List<TableModifier> tableModifiers;
        public List<PathModifier> pathModifiers;
        public List<DeckModifier> deckModifiers;
        [Tooltip("All Modifiers from all the Rules will be applied on completing the Act.")]
        public List<Rule> modRules;

        [Header("Slots")]
        public bool ignoreGlobalSlots;
        public List<Slot> slots;

        [Header("Alt Acts")]
        public bool randomAltAct;
        public List<ActLink> altActs;
        [Header("Next Acts")]
        public bool randomNextAct;
        public List<ActLink> nextActs;

        [Space(10)]
        [TextArea(3, 10)] public string text;
        [TextArea(3, 10)] public string endText;

        public bool Attempt(Context context)
        {
            foreach (var test in tests)
            {
                var r = test.Attempt(context);
                if (test.canFail == false && r == false)
                {
                    return false;
                }
            }

            foreach (var rule in testRules)
            {
                if (rule != null)
                {
                    var r = rule.Evaluate(context);
                    if (r == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void RunOnEnterRules(Context context)
        {
            foreach (var rule in onEnterRules)
            {
                rule?.Run(context);
            }
        }

        public void ApplyModifiers(Context context)
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
            }
        }
    }

    [Serializable]
    public class ActLink
    {
        [Tooltip("% chance of attempting this Act. Attemtping does not equal succeeding and Act's entry Tests must be passed. If there is only one element 0% becomes 100%")]
        [Range(0, 100)] public int chance;
        public Act act;
    }
}
