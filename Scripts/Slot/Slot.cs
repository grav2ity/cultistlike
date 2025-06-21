using System;
using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    [CreateAssetMenu(menuName = "Slot")]
    public class Slot : ScriptableObject
    {
        public string label;
        [Space(10)]
        [TextArea(3, 10)] public string description;
        [Space(10)]
        [Tooltip("Fragments will be added to the Act window whenever Card is slotted.")]
        public List<Fragment> fragments;

        [Header("Spawn")]
        [Tooltip("Token in which Slot will attempt to spawn.")]
        public Token token;
        [Tooltip("Only one instance of this Slot can be opened per window.")]
        public bool unique;
        [Tooltip("Attempt to spawn in all Tokens.")]
        public bool allTokens;
        [Tooltip("Attempt to spawn in all running Acts.")]
        public bool allActs;

        [Header("Spawn Tests")]
        [Tooltip("All the Tests must pass for this Slot to spawn.")]
        public List<Test> spawnTests;
        [Tooltip("Rule must pass for this Slot to spawn.")]
        public Rule spawnRule;

        [Header("Accepted Fragments")]
        [Tooltip("Card must have at least Count of one of the Required Fragments to be accepted in this Slot.")]
        public List<HeldFragment> required;
        [Tooltip("Card must have at least Count for all the Essential Fragments to be accepted in this Slot.")]
        public List<HeldFragment> essential;
        [Tooltip("Card must have less that Count for every Forbidden Fragment to be accepted in this Slot.")]
        public List<HeldFragment> forbidden;

        [Header("Card Rule")]
        [Tooltip("Additional Rule that must pass for a Card to be accepted. This will not show in the tooltip.")]
        public Rule cardRule;

        [Header("Options")]
        [Tooltip("Allows to place all Cards in the Slot.")]
        public bool acceptAll;
        [Tooltip("Slot will automatically grab Cards for itself.")]
        public bool grab;
        [Tooltip("Cannot remove Card from the Slot.")]
        public bool cardLock;
        public bool actGrab;


        public bool Opens(ActLogic actLogic)
        {
            if (spawnTests.Count == 0 && spawnRule == null)
            {
                return true;
            }
            else
            {
                var context = new Context(actLogic);
                foreach (var test in spawnTests)
                {
                    var r = test.Attempt(context);
                    if (test.canFail == false && r == false)
                    {
                        return false;
                    }
                }

                if (spawnRule == null)
                {
                    return true;
                }
                else
                {
                    context.ResetMatches();
                    return spawnRule.Evaluate(context);
                }
            }
        }

        public bool CheckFragRules(CardViz cardViz)
        {
            foreach (var fragL in essential)
            {
                if (cardViz.fragTree.Count(fragL) < fragL.count)
                {
                    return false;
                }
            }
            foreach (var fragL in forbidden)
            {
                if (cardViz.fragTree.Count(fragL) >= fragL.count)
                {
                    return false;
                }
            }
            foreach (var fragL in required)
            {
                if (cardViz.fragTree.Count(fragL) >= fragL.count)
                {
                    return true;
                }
            }
            return (required.Count == 0 ? true : false);
        }

        public bool AcceptsCard(CardViz cardViz)
        {
            if (acceptAll == true)
            {
                return true;
            }

            if (cardViz != null && CheckFragRules(cardViz) == true)
            {
                if (cardRule == null)
                {
                    return true;
                }
                else
                {
                    var context = new Context(cardViz);
                    return cardRule.Evaluate(context);
                }
            }
            return false;
        }
    }
}
