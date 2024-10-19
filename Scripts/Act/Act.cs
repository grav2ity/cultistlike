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
        [Space(10)]
        public List<Aspect> aspects;

        [Header("Entry Tests")]
        [Tooltip("All Tests must pass to enter this Act.")]
        public List<Test> tests;

        [Header("Modifiers")]
        public List<ActModifier> actModifiers;
        public List<CardModifier> cardModifiers;
        public List<TableModifier> tableModifiers;

        [Space(10)]
        [Header("Followup Rules")]
        [Tooltip("All Tests from all Rules must pass to enter. All Modifiers will be applied on completing the Act.")]
        public List<Rule> rules;

        [Header("Slots")]
        public List<Slot> slots;
        public bool spawnGlobalSlots;

        [Header("Acts")]
        public List<ActLink> altActs;
        public List<ActLink> nextActs;

        [Space(10)]
        [TextArea(3, 10)] public string text;
        [TextArea(3, 10)] public string endText;

        public bool Attempt(FragContainer scope)
        {
            scope.matches = scope.cards;

            foreach (var test in tests)
            {
                var r = test.Attempt(scope);
                if (test.canFail == false && r == false)
                {
                    return false;
                }
            }

            foreach (var rule in rules)
            {
                scope.matches = scope.cards;
                var r = rule.Attempt(scope);
                if (r == false)
                {
                    return false;
                }
            }

            return true;
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
