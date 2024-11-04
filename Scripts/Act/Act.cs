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
        [Tooltip("All of the AND Rules must pass. Modifiers are not applied.")]
        public List<Rule> and;
        [Tooltip("One of the OR Rules must pass. Modifiers are not applied.")]
        public List<Rule> or;

        [Header("Fragments")]
        public List<Fragment> fragments;

        [Header("On Complete")]
        public List<ActModifier> actModifiers;
        public List<CardModifier> cardModifiers;
        public List<TableModifier> tableModifiers;
        public List<PathModifier> pathModifiers;
        public List<DeckModifier> deckModifiers;

        [Header("Furthermore")]
        public List<Rule> furthermore;

        [Header("Slots")]
        public bool ignoreGlobalSlots;
        public List<Slot> slots;

        [Header("Alt Acts")]
        public bool randomAlt;
        public List<ActLink> altActs;
        [Header("Next Acts")]
        public bool randomNext;
        public List<ActLink> nextActs;
        [Header("Spawned Acts")]
        public List<ActLink> spawnedActs;

        [Header("On Spawn")]
        public Rule onSpawn;

        [Space(10)]
        [TextArea(3, 10)] public string text;
        [TextArea(3, 10)] public string endText;

        public bool Attempt(Context context) => Rule.Evaluate(context, tests, and, or);

        public void ApplyModifiers(Context context) => Rule.Execute(context, actModifiers,
                                                                    cardModifiers, tableModifiers,
                                                                    pathModifiers, deckModifiers,
                                                                    furthermore);
    }

    [Serializable]
    public class ActLink
    {
        [Tooltip("% chance of attempting this Act. If there is only one element 0% becomes 100%")]
        [Range(0, 100)] public int chance;
        public Act act;
        [Tooltip("Rule's tests must pass to attempt this Act. If this is set, 'Chance' field is disregarded.")]
        public Rule actRule;
    }
}
