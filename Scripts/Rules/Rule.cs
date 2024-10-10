using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;


namespace CultistLike
{
    [Serializable]
    public struct Requirement
    {
        public string name;
        public Card card;
        [Tooltip("If specific Card is not set any card with the following Aspects will do")]
        public List<Aspect> aspects;

        public bool AttemptOne(Card c)
        {
            if (c == null)
            {
                return false;
            }
            if (card != null && card == c)
            {
                return true;
            }
            else
            {
                if (aspects.Count == 0)
                {
                    return false;
                }

                var asp = aspects.Except(c.aspects);
                return (asp.Count() == 0);
            }
        }
    }

    [Serializable]
    public struct Result
    {
        [Tooltip("Has no effect if Resulsts have only one element")]
        [Range(0, 1)] public float chance;
        [Space(10)]
        public Act nextAct;
        [Space(10)]
        public List<ActModifier> actModifiers;
        public List<AspectModifier> aspectModifiers;
        public List<CardModifier> cardModifiers;
        public List<TableModifier> tableModifiers;
        [Space(10)]
        [TextArea(3, 10)] public string endText;
        }

    [CreateAssetMenu(menuName = "Rule")]
    public class Rule : ScriptableObject
    {
        public float time;
        public List<Requirement> requirements;

        public List<Result> results;

        [TextArea(3, 10)] public string startText;
        [Tooltip("If not set defaults to Start Text")]
        [TextArea(3, 10)] public string runText;
        [Tooltip("Used if Result's End Text is not set")]
        [TextArea(3, 10)] public string endText;


        /// <summary>
        /// Attempt to fulfil the rule with the provided cards.
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public bool Attempt(List<Card> cards)
        {
            if (requirements.Count == 0)
            {
                return true;
            }

            //TODO
            if (cards == null || requirements == null || cards.Count != requirements.Count)
            {
                return false;
            }

            for (int i=0; i<requirements.Count; i++)
            {
                if (requirements[i].AttemptOne(cards[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public bool AttemptOne(int i, Card card) => i >= 0 &&
            i < requirements.Count && requirements[i].AttemptOne(card);

        public bool AttemptFirst(Card card) => AttemptOne(0, card);

        public Result GenerateResults()
        {
            float r = Random.Range(0.0f, 1.0f);

            float f = 0.0f;
            foreach (Result result in results)
            {
                if (result.chance == 0.0f)
                {
                    f = 1.0f;
                }
                else
                {
                    f = f + result.chance;
                }

                if (r <= f)
                {
                    return result;
                }
            }

            //TODO
            if (results.Count > 0)
            {
                return results[0];
            }
            else
            {
                Debug.LogWarning("Rule's Results List is empty!");
                return new Result();
            }
        }

    }
}
