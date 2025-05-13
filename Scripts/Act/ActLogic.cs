using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;


namespace CultistLike
{
    public class ActLogic : MonoBehaviour
    {
        public FragTree fragTree;

        [SerializeField, HideInInspector] private Act _activeAct;
        [SerializeField, HideInInspector] private Act _altAct;

        [SerializeField, HideInInspector] private List<Act> altActs;
        [SerializeField, HideInInspector] private List<Act> nextActs;
        [SerializeField, HideInInspector] private List<Act> spawnedActs;

        // [SerializeField, HideInInspector] private List<Act> extraAltActs;
        // [SerializeField, HideInInspector] private List<Act> extraNextActs;

        [SerializeField, HideInInspector] private Act forceAct;
        [SerializeField, HideInInspector] private Act callbackAct;
        [SerializeField, HideInInspector] private bool doCallback;
        [SerializeField, HideInInspector] private Rule forceRule;

        [SerializeField, HideInInspector] private string _endText;

        [SerializeField, HideInInspector] private Act branchOutAct;
        private Stack<Act> callStack;

        private ActWindow actWindow;


        public Act activeAct { get => _activeAct; private set => _activeAct = value; }
        public Act altAct { get => _altAct; set => _altAct = value; }

        public FragTree slotsFragTree { get => actWindow.slotsFragTree; }

        public TokenViz tokenViz { get => actWindow.tokenViz; }

        // public string endText => fragTree.InterpolateString(_endText);
        public string endText => _endText;
        public string runText => altAct ? GetText(altAct) : GetText(activeAct);


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
                        if (slot != null && slot.allActs == true)
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

                foreach (var cardViz in fragTree.cards)
                {
                    foreach (var slot in cardViz.card.slots)
                    {
                        slotsToAttempt.Add(slot);
                    }
                }
                foreach (var frag in fragTree.fragments)
                {
                    foreach (var slot in frag.fragment.slots)
                    {
                        slotsToAttempt.Add(slot);
                    }
                }

                foreach (var slot in GameManager.Instance.slotSOS)
                {
                    if (slot != null && slot.allTokens == true || slot.token == tokenViz?.token)
                    {
                        slotsToAttempt.Add(slot);
                    }
                }
            }

            foreach (var slot in slotsToAttempt)
            {
                if (slot != null && slot.unique == false || slotsToOpen.Contains(slot) == false)
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
            branchOutAct = null;

            if (forceRule != null)
            {
                using (var context = new Context(this))
                {
                    forceRule.Run(context);
                }
                forceRule = null;
            }

            actWindow.ParentSlotCardsToWindow();
            //need this for correct slots
            actWindow.ApplyStatus(ActStatus.Running);
            actWindow.UpdateSlots();

            PopulateActList(act.altActs, altActs, act.randomAlt);
            // foreach (var altAct in extraAltActs)
            // {
            //     altActs.Insert(0, altAct);
            // }
            // extraAltActs.Clear();

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

            // extraAltActs.Clear();
            // extraNextActs.Clear();

            _endText = "";

            fragTree.Clear();
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

            actWindow.ParentSlotCardsToWindow();

            foreach (var frag in activeAct.fragments)
            {
                fragTree.Add(frag);
            }

            ApplyTriggers();

            using (var context = new Context(this, true))
            {
                activeAct.ApplyModifiers(context);
            }

            PopulateActList(activeAct?.spawnedActs, spawnedActs, false);
            foreach (var spawnedAct in spawnedActs)
            {
                GameManager.Instance.SpawnAct(spawnedAct, fragTree, tokenViz);
            }

            actWindow.ParentSlotCardsToWindow();

            var et = GetEndText(activeAct);
            if (et != "")
            {
                _endText = et;
            }

            if (forceAct != null)
            {
                ForceAct(forceAct);
            }
            else if (branchOutAct != null)
            {
                Context context = new Context(this);
                if (AttemptAct(branchOutAct, context) == true)
                {
                    Debug.Log("Branching out to act: " + branchOutAct.name);
                    callStack.Push(activeAct);
                    RunAct(branchOutAct);
                }
            }
            else
            {
                if (doCallback == true && callbackAct != null)
                {
                    doCallback = false;
                    Context context = new Context(this);
                    if (AttemptAct(callbackAct, context) == true)
                    {
                        RunAct(callbackAct);
                        return;
                    }
                }

                PopulateActList(activeAct.nextActs, nextActs, activeAct.randomNext);
                // foreach (var act in extraNextActs)
                // {
                //    nextActs.Insert(0, act);
                // }
                // extraNextActs.Clear();

                var nextAct = AttemptNextActs();
                if (nextAct == null && callStack.Count > 0)
                {
                    var act = callStack.Pop();
                    PopulateActList(act.nextActs, nextActs, act.randomNext);
                    nextAct = AttemptNextActs();
                }

                if (nextAct != null)
                {
                    RunAct(nextAct);
                }
                else
                {
                    SetupFinalResults();
                }
            }
        }

        private void SetupFinalResults()
        {
            //TODO this cuts short DOTween animations
            actWindow.SetupResultCards(fragTree.cards);
            actWindow.ApplyStatus(ActStatus.Finished);
        }

        public void ForceAct(Act act)
        {
            Context context = new Context(this);
            //just to save matches
            AttemptAct(act, context, true);
            RunAct(act);
        }

        public void SetCallback(Act act) => callbackAct = act;
        public void DoCallback() => doCallback = true;
        public void SetForceAct(Act act) => forceAct = act;
        public void ForceRule(Rule rule) => forceRule = rule;

        public void BranchOut(Act act)
        {
            if (act != null)
            {
                branchOutAct = act;
            }
        }

        // public void InjectNextAct(Act act) => extraNextActs.Add(act);
        // public void InjectAltAct(Act act) => extraAltActs.Add(act);

        public Act AttemptInitialActs() => AttemptActs(GameManager.Instance.initialActs, true);
        public Act AttemptAltActs() => AttemptActs(altActs);
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

        public void InjectTriggers(Target target)
        {
            if (target != null)
            {
                using (var context = new Context(this))
                {
                    if (target.fragment != null)
                    {
                        ApplyAspectTrigers(context, target.fragment);
                    }
                    else
                    {
                        foreach (var cardViz in target.cards)
                        {
                            ApplyCardTriggers(context, cardViz);
                        }
                    }
                }
            }
        }

        private void ApplyCardTriggers(Context context, CardViz cardViz)
        {
            if (cardViz != null)
            {
                context.thisCard = cardViz;
                context.thisAspect = cardViz.card;
                foreach (var rule in cardViz.card.rules)
                {
                    if (rule) rule.Run(context);
                }
            }
        }

        private void ApplyAspectTrigers(Context context, Fragment fragment)
        {
            if (fragment != null)
            {
                context.thisAspect = fragment;
                foreach (var rule in fragment.rules)
                {
                    if (rule) rule.Run(context);
                }
            }
        }

        private void ApplyTriggers()
        {
            using (var context = new Context(this))
            {
                foreach (var cardViz in context.scope.cards)
                {
                    ApplyCardTriggers(context, cardViz);

                }
                context.thisCard = null;
                foreach (var frag in context.scope.fragments)
                {
                    ApplyAspectTrigers(context, frag.fragment);
                }
            }
        }

        public string InterpolateString(string s) => fragTree.InterpolateString(s);

        public string TokenDesription() => tokenViz.token != null ? GetText(tokenViz.token.textRules, tokenViz.token.description) : "";

        public string GetText(Act act) => GetText(act.textRules, act.text);
        public string GetEndText(Act act) => GetText(act.endTextRules, act.endText);

        private string GetText(List<Rule> textRules, string text)
        {
            if (textRules != null && textRules.Count > 0)
            {
                Context context = new Context(this);

                foreach (var rule in textRules)
                {
                    if (rule != null && rule.Evaluate(context) == true)
                    {
                        return InterpolateString(rule.text);
                    }
                }
            }

            return InterpolateString(text);
        }

        public ActLogicSave Save()
        {
            var save = new ActLogicSave();
            save.fragSave = fragTree.Save();

            save.activeAct = activeAct;
            save.callbackAct = callbackAct;
            save.branchOutAct = branchOutAct;
            save.callStack = callStack.ToList();
            save.altAct = altAct;

            save.endText = _endText;

            return save;
        }

        public void Load(ActLogicSave save)
        {
            fragTree.Load(save.fragSave);
            activeAct = save.activeAct;
            callbackAct = save.callbackAct;
            branchOutAct = save.branchOutAct;
            callStack = new Stack<Act>(save.callStack.AsEnumerable().Reverse());
            altAct = save.altAct;

            _endText = save.endText;
        }

        private void Awake()
        {
            actWindow = GetComponent<ActWindow>();
            fragTree = GetComponent<FragTree>();
            fragTree.onCreateCard = x => x.ParentToWindow(actWindow.transform, true);

            callStack = new Stack<Act>();
        }
    }

    [Serializable]
    public class ActLogicSave
    {
        public FragTreeSave fragSave;
        public Act activeAct;
        public Act callbackAct;
        public Act branchOutAct;
        public List<Act> callStack;

        //???
        public Act altAct;

        public string endText;
    }

}
