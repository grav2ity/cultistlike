﻿using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;


namespace CultistLike
{
    public class ActLogic : MonoBehaviour
    {
        [SerializeField] private FragContainer _fragments;

        [SerializeField, HideInInspector] private Act _activeAct;
        [SerializeField, HideInInspector] private Act _altAct;

        // [SerializeField, HideInInspector] private List<Act> altActs;
        // [SerializeField, HideInInspector] private List<Act> nextActs;
        [SerializeField, HideInInspector] public List<Act> altActs;
        [SerializeField, HideInInspector] public List<Act> nextActs;
        [SerializeField, HideInInspector] public List<Act> spawnedActs;

        [SerializeField, HideInInspector] private Act forceAct;
        [SerializeField, HideInInspector] private Rule forceRule;

        public ActWindow actWindow;

        public ActLogic parent;
        public List<ActLogic> children;


        public FragContainer fragments { get => _fragments; private set => _fragments = value; }
        public Act activeAct { get => _activeAct; private set => _activeAct = value; }
        public Act altAct { get => _altAct; set => _altAct = value; }

        public TokenViz tokenViz { get => actWindow.tokenViz; }


        public List<Slot> CheckForSlots()
        {
            List<Slot> slotsToAttempt = new List<Slot>();
            List<Slot> slotsToOpen = new List<Slot>();

            if (activeAct != null)
            {
                foreach (var slot in activeAct.slots)
                {
                    slotsToAttempt.Add(slot);
                }

                if (activeAct.ignoreGlobalSlots == false)
                {
                    foreach (var slot in GameManager.Instance.slotSOS)
                    {
                        if (slot.allActs == true)
                        {
                            slotsToAttempt.Add(slot);
                        }
                    }
                }
            }
            else
            {
                if (actWindow.tokenViz?.token?.slot != null)
                {
                    slotsToOpen.Add(actWindow.tokenViz?.token?.slot);
                }

                foreach (var cardViz in fragments.cards)
                {
                    foreach (var slot in cardViz.card.slots)
                    {
                        slotsToAttempt.Add(slot);
                    }
                }
                foreach (var frag in fragments.fragments)
                {
                    foreach (var slot in frag.fragment.slots)
                    {
                        slotsToAttempt.Add(slot);
                    }
                }

                foreach (var slot in GameManager.Instance.slotSOS)
                {
                    if (slot.allTokens == true || slot.token == actWindow.tokenViz?.token)
                    {
                        slotsToAttempt.Add(slot);
                    }
                }
            }

            foreach (var slot in slotsToAttempt)
            {
                if (slot.unique == false || slotsToOpen.Contains(slot) == false)
                {
                    if (slot.Opens(this) == true)
                    {
                        slotsToOpen.Add(slot);
                    }
                }
            }
            return slotsToOpen;
        }


        public void RunAct(Act act)
        {
            Debug.Log("Running act: " + act.name);
            activeAct = act;
            forceAct = null;

            if (forceRule != null)
            {
                using (var context = new Context(this))
                {
                    forceRule.Run(context);
                }
                forceRule = null;
            }

            foreach (var frag in activeAct.fragments)
            {
                fragments.Add(frag);
            }

            ParentCardsToWindow();
            actWindow.UpdateBars();
            //need this for correct slots
            actWindow.ApplyStatus(ActStatus.Running);
            actWindow.UpdateSlots();

            PopulateActList(act.altActs, altActs, act.randomAlt);

            altAct = AttemptAltActs();

            if (act.time > 0)
            {
                tokenViz.timer.StartTimer(act.time, () =>
                {
                    tokenViz.ShowTimer(false);
                    SetupActResults();
                });
                tokenViz.ShowTimer(true);
                actWindow.ApplyStatus(ActStatus.Running);
            }
            else
            {
                SetupActResults();
            }
        }

        public void Reset()
        {
            activeAct = null;
            altAct = null;

            fragments.Clear();
        }

        private void PopulateActList(List<ActLink> source, List<Act> target, bool randomOrder = false)
        {
            if (source != null && target != null)
            {
                target.Clear();

                if (source.Count == 1 && source[0].chance == 0 && source[0].actRule == null)
                {
                    target.Add(source[0].act);
                }
                else
                {
                    foreach (var actLink in source)
                    {
                        bool passed = false;
                        if (actLink.actRule != null)
                        {
                            using (var context = new Context(this))
                            {
                                passed = actLink.actRule.Evaluate(context);
                            }
                        }
                        else
                        {
                            int r = Random.Range(0, 100);
                            if (r < actLink.chance)
                            {
                                passed = true;
                            }
                        }
                        if (passed == true)
                        {
                            if (randomOrder == false)
                            {
                                target.Add(actLink.act);
                            }
                            else
                            {
                                int i = Random.Range(0, target.Count + 1);
                                target.Insert(i, actLink.act);
                            }
                        }
                    }
                }
            }
        }

        private void SetupActResults()
        {
            if (altAct != null)
            {
                Debug.Log("Switched to: " + altAct.name);
                activeAct = altAct;
            }

            ApplyCardRules();

            using (var context = new Context(this, true))
            {
                activeAct.ApplyModifiers(context);
            }

            PopulateActList(activeAct?.spawnedActs, spawnedActs, false);
            foreach (var spawnedAct in spawnedActs)
            {
                GameManager.Instance.SpawnAct(spawnedAct, this);
            }

            ParentCardsToWindow();
            actWindow.UpdateBars();

            if (forceAct != null)
            {
                RunAct(forceAct);
            }
            else
            {
                PopulateActList(activeAct.nextActs, nextActs, activeAct.randomNext);
                var nextAct = AttemptNextActs();
                if (nextAct != null)
                {
                    RunAct(nextAct);
                }
                else
                {
                    SetupFinalResults(activeAct.endText);
                }
            }
        }

        private void SetupFinalResults(string endText)
        {
            if (fragments.cards != null)
            {
                actWindow.SetupResultCards(fragments.cards);
            }

            activeAct = null;
        }

        public void ForceAct(Act act) => forceAct = act;
        public void ForceRule(Rule rule) => forceRule = rule;

        public Act AttemptAct(Act act)
        {
            if (act != null)
            {
                if (act.token != null && act.token != actWindow.tokenViz.token)
                {
                    return null;
                }
                var context = new Context(this);
                if (act.Attempt(context) == true)
                {
                    return act;
                }
            }
            return null;
        }

        private Act AttemptActs(List<Act> acts)
        {
            Context context = new Context(this);
            Act pAct = null;
            foreach (var act in acts)
            {
                context.ResetMatches();
                if (act != null && act.Attempt(context) == true)
                {
                    pAct = act;
                    context.SaveMatches();
                    break;
                }
            }
            return pAct;
        }

        public Act AttemptAltActs() => AttemptActs(altActs);
        private Act AttemptNextActs() => AttemptActs(nextActs);

        private void ApplyCardRules()
        {
            using (var context = new Context(this))
            {
                foreach (var cardViz in context.scope.cards)
                {
                    context.card = cardViz;
                    foreach (var rule in cardViz.card.rules)
                    {
                        rule?.Run(context);
                    }

                    foreach (var frag in cardViz.fragments.fragments)
                    {
                        if (frag.fragment != null)
                        {
                            foreach (var rule in frag.fragment.rules)
                            {
                                rule?.Run(context);
                            }
                        }
                    }
                }
            }
        }

        public void HoldCard(CardViz cardViz, Slot slot = null)
        {
            if (cardViz != null)
            {
                fragments.Add(cardViz);
                if (slot != null)
                {
                    foreach (var frag in slot.fragments)
                    {
                        fragments.Add(frag);
                    }
                }
                actWindow.UpdateBars();
                altAct = AttemptAltActs();
            }
        }

        public CardViz UnholdCard(CardViz cardViz, Slot slot = null)
        {
            if (cardViz != null)
            {
                fragments.Remove(cardViz);
                if (slot != null)
                {
                    foreach (var frag in slot.fragments)
                    {
                        fragments.Remove(frag);
                    }
                }

                actWindow.UpdateBars();
                altAct = AttemptAltActs();
            }
            return cardViz;
        }

        private void ParentCardsToWindow()
        {
            actWindow.ParentCardsToWindow();
            foreach (var cardViz in fragments.cards)
            {
                cardViz.gameObject.SetActive(false);
                cardViz.transform.SetParent(transform);
            }
        }

        public void SetParent(ActLogic parent)
        {
            if (this.parent != null)
            {
                this.parent.RemoveChild(this);
            }
            if (parent != null)
            {
                parent.AddChild(this);
            }
            this.parent = parent;
        }

        public void AddChild(ActLogic child)
        {
            if (child != null && children.Contains(child) == false)
            {
                children.Add(child);
            }
        }

        public void RemoveChild(ActLogic child)
        {
            if (child != null)
            {
                children.Remove(child);
            }
        }

        private void Awake()
        {
            actWindow = GetComponent<ActWindow>();
        }
    }
}
