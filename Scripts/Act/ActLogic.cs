using System;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;


namespace CultistLike
{
    public class ActLogic : MonoBehaviour
    {
        [SerializeField] private FragContainer _fragments;

        [SerializeField, HideInInspector] private Act _activeAct;
        [SerializeField, HideInInspector] private Act _altAct;

        [SerializeField, HideInInspector] private List<Act> altActs;
        [SerializeField, HideInInspector] private List<Act> nextActs;
        [SerializeField, HideInInspector] private List<Act> spawnedActs;

        [SerializeField, HideInInspector] private Act forceAct;
        [SerializeField, HideInInspector] private Rule forceRule;

        [SerializeField, HideInInspector] private ActLogic _parent;
        [SerializeField, HideInInspector] private List<ActLogic> children;

        private ActWindow actWindow;


        public FragContainer fragments { get => _fragments; private set => _fragments = value; }
        public Act activeAct { get => _activeAct; private set => _activeAct = value; }
        public Act altAct { get => _altAct; set => _altAct = value; }

        public ActLogic parent  { get => _parent; private set => _parent = value; }

        public TokenViz tokenViz { get => actWindow.tokenViz; }


        /// <summary>
        /// Returns a list of Slots to open.
        /// </summary>
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
                if (tokenViz?.token?.slot != null)
                {
                    slotsToOpen.Add(tokenViz?.token?.slot);
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
                    if (slot.allTokens == true || slot.token == tokenViz?.token)
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

            actWindow.ParentSlotCardsToWindow();
            actWindow.UpdateBars();
            //need this for correct slots
            actWindow.ApplyStatus(ActStatus.Running);
            actWindow.UpdateSlots();

            PopulateActList(act.altActs, altActs, act.randomAlt);

            altAct = AttemptAltActs();

            if (act.time > 0)
            {
                tokenViz.timer.StartTimer(act.time, OnTimeUp);
                tokenViz.ShowTimer(true);
                actWindow.ApplyStatus(ActStatus.Running);
            }
            else
            {
                SetupActResults();
            }
        }

        public void OnTimeUp()
        {
            tokenViz.ShowTimer(false);
            SetupActResults();
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

        public void SetupActResults()
        {
            if (altAct != null)
            {
                Debug.Log("Switched to: " + altAct.name);
                activeAct = altAct;
            }

            foreach (var frag in activeAct.fragments)
            {
                fragments.Add(frag);
            }

            ApplyTriggers();

            using (var context = new Context(this, true))
            {
                activeAct.ApplyModifiers(context);
            }

            PopulateActList(activeAct?.spawnedActs, spawnedActs, false);
            foreach (var spawnedAct in spawnedActs)
            {
                GameManager.Instance.SpawnAct(spawnedAct, this);
            }

            actWindow.ParentSlotCardsToWindow();
            actWindow.UpdateBars();

            if (forceAct != null)
            {
                ForceAct(forceAct);
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
            actWindow.ApplyStatus(ActStatus.Finished);
        }

        // public void AddNextAct(Act act)
        // {
        //     if (nextActs.Contains(act) == false)
        //     {
        //         nextActs.Add(act);
        //     }
        // }

        public void ForceAct(Act act)
        {
            Context context = new Context(this);
            //just to save matches
            AttemptAct(act, context, true);
            RunAct(act);
        }

        public void SetForceAct(Act act) => forceAct = act;
        public void ForceRule(Rule rule) => forceRule = rule;

        public Act AttemptInitialActs() => AttemptActs(GameManager.Instance.initialActs, true);
        private Act AttemptAltActs() => AttemptActs(altActs);
        private Act AttemptNextActs() => AttemptActs(nextActs);

        private bool AttemptAct(Act act, Context context, bool force = false)
        {
            context.ResetMatches();
            if (act.Attempt(context, force) == true)
            {
                context.SaveMatches();
                return true;
            }
            else
            {
                return false;
            }
        }

        private Act AttemptActs(List<Act> acts, bool matchToken = false)
        {
            Context context = new Context(this);
            foreach (var act in acts)
            {
                if (act != null)
                {
                    if (matchToken == true && act.token != tokenViz.token)
                    {
                        continue;
                    }

                    if (AttemptAct(act, context) == true)
                    {
                        return act;
                    }
                }
            }
            return null;
        }

        private void ApplyTriggers()
        {
            using (var context = new Context(this))
            {
                foreach (var cardViz in context.scope.cards)
                {
                    context.thisCard = cardViz;
                    foreach (var rule in cardViz.card.rules)
                    {
                        rule?.Run(context);
                    }

                }
                context.thisCard = null;
                foreach (var frag in context.scope.fragments)
                {
                    if (frag.fragment != null)
                    {
                        context.thisAspect = frag.fragment;
                        foreach (var rule in frag.fragment.rules)
                        {
                            rule?.Run(context);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds CardViz to the scope.
        /// </summary>
        /// <param name="cardViz">CardViz to add.</param>
        /// <param name="slot">Slot type if CardViz has been slotted.</param>
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

        /// <summary>
        /// Removes CardViz from the scope.
        /// </summary>
        /// <param name="cardViz">CardViz to remove.</param>
        /// <param name="slot">Slot type if CardViz has been unslotted.</param>
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

        public void ParentCardToWindow(CardViz cardViz)
        {
            cardViz.Hide();
            cardViz.free = false;
            cardViz.transform.SetParent(transform);
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

        public ActLogicSave Save()
        {
            var save = new ActLogicSave();
            save.fragSave = fragments.Save();
            save.activeAct = activeAct;
            save.altAct = altAct;
            return save;
        }

        public void Load(ActLogicSave save)
        {
            fragments.Load(save.fragSave);
            foreach (var cardViz in fragments.cards)
            {
                cardViz.fragments.parent = fragments;
                if (actWindow.actStatus == ActStatus.Running)
                {
                    if (cardViz.transform.parent == null)
                    {
                        ParentCardToWindow(cardViz);
                    }
                }
            }
            activeAct = save.activeAct;
            altAct = save.altAct;
        }


        private void AddChild(ActLogic child)
        {
            if (child != null && children.Contains(child) == false)
            {
                children.Add(child);
            }
        }

        private void RemoveChild(ActLogic child)
        {
            if (child != null)
            {
                children.Remove(child);
            }
        }

        private void Awake()
        {
            actWindow = GetComponent<ActWindow>();
            fragments.onCreateCard = actWindow.ParentCardToWindow;
        }
    }

    [Serializable]
    public class ActLogicSave
    {
        public FragContainerSave fragSave;
        public Act activeAct;
        //???
        public Act altAct;
        //parent children
    }

}
