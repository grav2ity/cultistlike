using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace CultistLike
{
    [CreateAssetMenu(menuName = "Act")]
    public class Act : ScriptableObject
    {
        public string actName;
        public Sprite art;
        public Color color;
        [Space(10)]
        [TextArea(3, 10)] public string text;

        [Header("Linked Act only")]
        public float time;
        public string slotTitle;

        [Header("Rules")]
        public List<Rule> rules;

        [Header("Consuming")]
        [Tooltip("Grabs cards (e.g. resources) from the table and destroys them")]
        public Rule consumeRule;
        [Tooltip("Called when no matching card could be found. Game Over could be set up here")]
        public UnityEvent onConsumeFail;


        /// <summary>
        /// Attempt a <c>Card</c> against first requirement of each rule.
        /// </summary>
        /// <param name="card"></param>
        /// <returns>Rules that can be initiatied with the provided <c>Card.</c></returns>
        public List<Rule> AttemptFirst(Card card)
        {
            List<Rule> possibleRules = new List<Rule>();
            foreach (Rule rule in rules)
            {
                if (rule == null)
                {
                    Debug.LogWarning("Missing Rule in " + actName);
                    continue;
                }
                if (rule.AttemptFirst(card) == true)
                {
                    possibleRules.Add(rule);
                }
            }
            return possibleRules;
        }
    }
}
