using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Act")]
    public class Act : ScriptableObject
    {
        public string label;
        [Space(10)]
        [Tooltip("Limits execution to this Token. Mandatory for initial and spawned Acts.")]
        public Token token;
        [Tooltip("First in the Act chain. Can be started by player pressing button.")]
        public bool initial;
        [Space(10)]
        public float time;

        [Header("Entry Tests")]
        [Tooltip("All the tests must pass to enter this Act. Matched Cards from Card tests will be available in the On Complete modifiers.")]
        public List<Test> tests;
        [Tooltip("All of the And Rules must pass to enter this Act. Modifiers are not applied. Matched Cards are not carried in nor out.")]
        public List<Rule> and;
        [Tooltip("One of the Or Rules must pass to enter this Act. Modifiers are not applied. Matched Cards are not carried in nor out.")]
        public List<Rule> or;

        [Header("Fragments")]
        [Tooltip("Fragments are added upon completing Act.")]
        public List<Fragment> fragments;

        [Header("On Complete")]
        public List<ActModifier> actModifiers;
        public List<CardModifier> cardModifiers;
        public List<TableModifier> tableModifiers;
        public List<PathModifier> pathModifiers;
        public List<DeckModifier> deckModifiers;

        [Header("Furthermore")]
        [Tooltip("Rules run upon completing Act. Matched Cards are not carried in nor out.")]
        public List<Rule> furthermore;

        [Header("Slots")]
        [Tooltip("Only Slots from the list below will attempt to open while Act is running.")]
        public bool ignoreGlobalSlots;
        [Tooltip("Additional Slots that will attempt to open while Act is running.")]
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
        [Tooltip("Rule that will be run on spawning this Act in a new Token.")]
        public Rule onSpawn;

        [Space(10)]
        [TextArea(3, 10)] public string text;
        [TextArea(3, 10)] public string endText;

        public bool Attempt(Context context, bool force = false) => Rule.Evaluate(context, tests, and, or, force);

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
