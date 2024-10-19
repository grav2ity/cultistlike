using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace CultistLike
{
    [CreateAssetMenu(menuName = "Slot")]
    public class Slot : ScriptableObject
    {
        public string title;
        [Space(10)]
        [TextArea(3, 10)] public string text;
        public List<Aspect> aspects;

        [Header("Spawn Tests")]
        [Tooltip("All the Tests must pass for this Slot to spawn.")]
        public List<Test> spawnTests;
        [Tooltip("One of the Rules must pass for this Slot to spawn.")]
        public List<Rule> spawnRules;

        [Header("Accepted Cards")]
        [Tooltip("All the Tests must pass to accept Card.")]
        public List<Test> cardTests;
        [Tooltip("One of the Rules must pass to accept Card.")]
        public List<Rule> cardRules;

        [Header("Options")]
        public bool onlyMatching;
        [Tooltip("Grabs Cards for itself.")]
        public bool grab;
        [Tooltip("Cannot remove Card from the slot.")]
        public bool cardLock;


        // public bool Opens(FragContainer scope)
        // {
        //     if (spawnTests.Count == 0)
        //     {
        //         return true;
        //     }
        //     else
        //     {
        //         scope.matches = scope.cards;

        //         foreach (var test in spawnTests)
        //         {
        //             var r = test.Attempt(scope);
        //             if (test.canFail == false && r == false)
        //             {
        //                 return false;
        //             }
        //         }

        //         return true;

        //         // foreach (var rule in spawnRules)
        //         // {
        //         //     if (rule.Attempt(scope) == true)
        //         //     {
        //         //         return true;
        //         //     }
        //         // }
        //     }

        //     return false;
        // }

        public bool Opens(FragContainer scope)
        {
            if (spawnTests.Count == 0 && spawnRules.Count == 0)
            {
                return true;
            }
            else
            {
                scope.matches.Clear();

                foreach (var test in spawnTests)
                {
                    var r = test.Attempt(scope);
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
                    if (rule != null && rule.Attempt(scope) == true)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool AcceptsCard(CardViz cardViz)
        {
            if (cardViz != null)
            {
                if (cardTests.Count == 0 && cardRules.Count == 0)
                {
                    return true;
                }
                else
                {
                    var scope = cardViz.fragments;
                    scope.matches.Clear();

                    foreach (var test in cardTests)
                    {
                        var r = test.Attempt(scope);
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
                        if (rule != null && rule.Attempt(scope) == true)
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
