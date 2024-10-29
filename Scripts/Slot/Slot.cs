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
        public List<Fragment> fragments;
        [Tooltip("Token in which Slot should attempt to spawn.")]

        [Header("Spawn")]
        public Token token;
        [Tooltip("Only one instance of this Slot can be opened per Window.")]
        public bool unique;
        [Tooltip("Attempt to spawn in all Tokens.")]
        public bool allTokens;
        [Tooltip("Attempt to spawn in all running Acts.")]
        public bool allActs;

        [Header("Spawn Tests")]
        [Tooltip("All the Tests must pass for this Slot to spawn.")]
        public List<Test> spawnTests;
        [Tooltip("One of the Rules must pass for this Slot to spawn.")]
        public List<Rule> spawnRules;

        [Header("Accepted Fragments")]
        [Tooltip("Card must have one of these to be accepted.")]
        public List<HeldFragment> required;
        [Tooltip("Card must have all of these to be accepted.")]
        public List<HeldFragment> essential;
        [Tooltip("Card must have none of these to be accepted.")]
        public List<HeldFragment> forbidden;

        [Header("Additional Card Tests")]
        [Tooltip("All the Tests must pass to accept Card. This will not show in the tooltip.")]
        public List<Test> cardTests;
        [Tooltip("One of the Rules must pass to accept Card. This will not show in the tooltip.")]
        public List<Rule> cardRules;

        [Header("Options")]
        public bool onlyMatching;
        [Tooltip("Grabs Cards for itself.")]
        public bool grab;
        [Tooltip("Cannot remove Card from the slot.")]
        public bool cardLock;


        public bool Opens(ActLogic actLogic)
        {
            if (spawnTests.Count == 0 && spawnRules.Count == 0)
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

                if (spawnRules.Count == 0)
                {
                    return true;
                }

                foreach (var rule in spawnRules)
                {
                    context.ResetMatches();
                    if (rule != null && rule.Evaluate(context) == true)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool CheckFragRules(CardViz cardViz)
        {
            foreach (var fragL in essential)
            {
                if (fragL.fragment != null && cardViz.fragments.Count(fragL.fragment) < fragL.count)
                {
                    return false;
                }
            }
            foreach (var fragL in forbidden)
            {
                if (fragL.fragment != null && cardViz.fragments.Count(fragL.fragment) >= fragL.count)
                {
                    return false;
                }
            }
            foreach (var fragL in required)
            {
                if (fragL.fragment != null && cardViz.fragments.Count(fragL.fragment) >= fragL.count)
                {
                    return true;
                }
            }
            return (required.Count == 0 ? true : false);
        }

        public bool AcceptsCard(CardViz cardViz)
        {
            if (cardViz != null && CheckFragRules(cardViz) == true)
            {
                if (cardTests.Count == 0 && cardRules.Count == 0)
                {
                    return true;
                }
                else
                {
                    var context = new Context(cardViz);
                    foreach (var test in cardTests)
                    {
                        var r = test.Attempt(context);
                        if (test.canFail == false && r == false)
                        {
                            return false;
                        }
                    }

                    if (cardRules.Count == 0)
                    {
                        return true;
                    }

                    foreach (var rule in cardRules)
                    {
                        context.ResetMatches();
                        if (rule != null && rule.Evaluate(context) == true)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
