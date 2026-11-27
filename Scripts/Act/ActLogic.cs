using System;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;


namespace CultistLike
{
    public class ActLogic : MonoBehaviour
    {
        public FragTree fragTree;

        public Action OnActRun;
        public Action OnActEnd;
        public Action OnAltAct;
        public Action<List<ActText>> OnNewText;
        public Action<List<ActText>> OnTextChange;
        public Action OnFinished;

        private Act _activeAct;
        private Act _altAct;

        [SerializeField, HideInInspector] private List<Act> altActs;
        [SerializeField, HideInInspector] private List<Act> nextActs;
        [SerializeField, HideInInspector] private List<Act> spawnedActs;

        private Act forceAct;
        private Act callbackAct;
        private bool doCallback;
        private Rule forceRule;

        [SerializeField, HideInInspector] private ActText lastText;
        [SerializeField, HideInInspector] private List<ActText> textHistory;

        [SerializeField, HideInInspector] private List<Act> branchOutActs;
        [SerializeField, HideInInspector] private CallTree callTree;


        private ActWindow actWindow;


        public Act activeAct { get => _activeAct; private set => _activeAct = value; }
        public Act altAct { get => _altAct; set => _altAct = value; }

        public FragTree slotsFragTree => actWindow.slotsFragTree;

        public TokenViz tokenViz => actWindow.tokenViz;


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
                        if (slot != null && slot.allActs)
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
                    slotsToOpen.Add(tokenViz.token.slot);
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
                    if (slot != null && slot.allTokens || slot.token == tokenViz?.token)
                    {
                        slotsToAttempt.Add(slot);
                    }
                }
            }

            foreach (var slot in slotsToAttempt)
            {
                if (slot != null && slot.unique == false || slotsToOpen.Contains(slot) == false)
                {
                    if (slot.Opens(this))
                    {
                        slotsToOpen.Add(slot);
                    }
                }
            }
            return slotsToOpen;
        }

        public void RunAct(Act act)
        {
            Debug.Log($"Running act: {act.name}", act);
            activeAct = act;
            forceAct = null;
            branchOutActs.Clear();

            callTree.act = act;

            if (forceRule != null)
            {
                using (var context = new Context(this))
                {
                    forceRule.Run(context);
                }
                forceRule = null;
            }

            NextText(GetText(activeAct), activeAct.label);

            OnActRun?.Invoke();

            PopulateActList(act.altActs, altActs, act.randomAlt);
            // foreach (var altAct in extraAltActs)
            // {
            //     altActs.Insert(0, altAct);
            // }
            // extraAltActs.Clear();

            AttemptAltActs();

            if (act.art != null)
            {
                tokenViz.SetArt(act.art);
            }

        #if UNITY_EDITOR
            if (act.time > 0 || GameManager.Instance.devTimeOn)
        #else
            if (act.time > 0)
        #endif
            {
                tokenViz.timer.StartTimer(act.time, OnTimeUp);
                tokenViz.ShowTimer();
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

            callTree = new();

            // extraAltActs.Clear();
            // extraNextActs.Clear();

            lastText = new();

            textHistory.Clear();

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
                        if (passed)
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
                Debug.Log("Switched to: {altAct.name}", altAct);
                activeAct = altAct;
            }

            if (activeAct.oneShot == true)
            {
                GameManager.Instance.oneShotActs.Add(activeAct);
            }

            //correct place??
            OnActEnd?.Invoke();


            foreach (var frag in activeAct.fragments)
            {
                fragTree.Add(frag);
            }

            //moved Context creation here for better clarity
            using (var context = new Context(this))
            {
                ApplyTriggers(context);
            }

            //each using scope batches modifier execution
            using (var context = new Context(this, true))
            {
                activeAct.ApplyModifiers(context);
            }

            PopulateActList(activeAct?.spawnedActs, spawnedActs);
            foreach (var spawnedAct in spawnedActs)
            {
                    //TODO update manual
                    using (var context = new Context(this))
                    {
                        if (AttemptAct(spawnedAct, context, false, false))
                        {
                            GameManager.Instance.SpawnAct(spawnedAct, fragTree, tokenViz);
                        }
                    }
            }


            NextText(GetEndText(activeAct), lastText.label);

            if (forceAct != null)
            {
                ForceAct(forceAct);
                return;
            }
            else if (branchOutActs.Count > 0)
            {
                List<Act> waiting = new();
                int i=0;
                for (; i < branchOutActs.Count; i++)
                {
                    using (var context = new Context(this))
                    {
                        if (AttemptAct(branchOutActs[i], context))
                            break;
                    }
                }

                if (i < branchOutActs.Count)
                {
                    waiting =  branchOutActs.GetRange(i, branchOutActs.Count);
                }

                if (waiting.Count > 0)
                {
                    foreach (var act in waiting)
                    {
                        CallTree newCallTree = new()
                        {
                            parent = callTree,
                            act = act
                        };
                        callTree.children.Add(newCallTree);
                    }

                    callTree = callTree.children[0];

                    Debug.Log($"Branching out to act: {callTree.act.name}", callTree.act);
                    RunAct(callTree.act);
                    return;
                }
            }


            if (doCallback && callbackAct != null)
            {
                doCallback = false;
                Context context = new Context(this);
                if (AttemptAct(callbackAct, context))
                {
                    RunAct(callbackAct);
                    return;
                }
            }

            PopulateActList(activeAct.nextActs, nextActs, activeAct.randomNext);
            Act nextAct = AttemptNextActs();

            while (nextAct == null && callTree.parent != null)
            {
                callTree.parent.children.RemoveAt(0);
                if (callTree.parent.children.Count > 0)
                {
                    callTree = callTree.parent.children[0];

                    using (var context = new Context(this))
                    {
                        if (AttemptAct(callTree.act, context))
                        {
                            nextAct = callTree.act;
                            Debug.Log($"Branching out to act: {callTree.act.name}", callTree.act);
                        }
                    }
                }
                else
                {
                    callTree = callTree.parent;
                    PopulateActList(callTree.act.nextActs, nextActs, callTree.act.randomNext);
                    nextAct = AttemptNextActs();
                }
            }


            if (nextAct != null)
            {
                RunAct(nextAct);
            }
            else
            {
                //TODO this cuts short DOTween animations
                OnFinished?.Invoke();
            }
        }

        public void ForceAct(Act act)
        {
            MakeMatches(act);

            callTree = new();
            branchOutActs.Clear();

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
                branchOutActs.Add(act);
            }
        }

        // public void InjectNextAct(Act act) => extraNextActs.Add(act);
        // public void InjectAltAct(Act act) => extraAltActs.Add(act);

        public Act AttemptInitialActs() => AttemptActs(GameManager.Instance.initialActs, true);
        public Act AttemptAltActs()
        {
            var newAct = AttemptActs(altActs);
            if (altAct != null)
            {
                if (newAct == null)
                {
                    altAct = null;
                    SwitchFromAlt();
                    OnAltAct?.Invoke();

                }
                if (newAct != altAct)
                {
                    altAct = newAct;
                    SwitchToAlt();
                    OnAltAct?.Invoke();
                }
            }
            else
            {
                if (newAct != null)
                {
                    altAct = newAct;
                    SwitchToAlt();
                    OnAltAct?.Invoke();
                }
            }
            return null;
        }

        private Act AttemptNextActs() => AttemptActs(nextActs);

        private bool AttemptAct(Act act, Context context, bool force = false, bool saveMatches = true)
        {
            //TODO correct place and what about force
            if (act.Available == false)
            {
                return false;
            }

            if (act.Attempt(context, force))
            {
                if (saveMatches == true)
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
                    if (matchToken && act.token != tokenViz.token)
                    {
                        continue;
                    }

                    if (AttemptAct(act, context))
                    {
                        return act;
                    }
                    context.ResetMatches();
                }
            }
            return null;
        }

        //make matchCards list for acts that were not attempted or need update
        private void MakeMatches(Act act)
        {
            Context context = new Context(this);
            //just to save matches
            AttemptAct(act, context, true);
        }

        public void InjectTriggers(Target target)
        {
            if (target != null)
            {
                using (var context = new Context(this))
                {
                    if (target.fragment != null)
                    {
                        ApplyAspectTriggers(context, target.fragment);
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
                    if (rule)
                    {
                        rule.Run(context);
                        context.ResetMatches();
                    }
                }
            }
        }

        private void ApplyAspectTriggers(Context context, Fragment fragment)
        {
            if (fragment != null)
            {
                context.thisAspect = fragment;
                foreach (var rule in fragment.rules)
                {
                    if (rule)
                    {
                        rule.Run(context);
                        context.ResetMatches();
                    }
                }
            }
        }

        private void ApplyTriggers(Context context)
        {
            foreach (var cardViz in context.scope.cards)
            {
                ApplyCardTriggers(context, cardViz);

            }
            context.thisCard = null;
            //TODO each fragment type gets only one trigger
            //which is or isn't a desired behaviour?
            foreach (var frag in context.scope.fragments)
            {
                ApplyAspectTriggers(context, frag.fragment);
            }
        }

        private void SwitchToAlt()
        {
            if (altAct != null)
            {
                if (textHistory.Count == 0)
                    textHistory.Add(new ActText());

                //altAct always changes displayed text even if strings are empty
                textHistory[textHistory.Count -1].text = GetText(altAct);
                textHistory[textHistory.Count -1].label = altAct.label;
                OnTextChange?.Invoke(textHistory);
            }
        }

        private void SwitchFromAlt()
        {
            //TODO rather lame temp solution
            MakeMatches(activeAct);

            textHistory[textHistory.Count -1].text = lastText.text;
            textHistory[textHistory.Count -1].label = lastText.label;
            OnTextChange?.Invoke(textHistory);
        }

        private void NextText(string text, string label)
        {
            //TODO new label and no new text bahaviour?
            if (text != "")
            {
                lastText.text = text;

                var nt = new ActText();
                nt.text = text;
                nt.label = label != "" ? label : lastText.label;
                lastText.label = nt.label;
                textHistory.Add(nt);

                OnNewText?.Invoke(textHistory);
                OnTextChange?.Invoke(textHistory);
            }
        }

        public ActText TextHistory(int i)
        {
            if (i < textHistory.Count)
            {
                return textHistory[i];

            }
            else
            {
                return null;
            }
        }


        public int TextHistoryCount => textHistory.Count;

        public string InterpolateString(string s) => fragTree.InterpolateString(s);

        public string TokenDescription() => tokenViz.token != null ? GetText(tokenViz.token.textRules, tokenViz.token.description) : "";

        public string GetText(Act act) => GetText(act.textRules, act.text);
        public string GetEndText(Act act) => GetText(act.endTextRules, act.endText);

        private string GetText(List<Rule> textRules, string text)
        {
            if (textRules != null && textRules.Count > 0)
            {
                Context context = new Context(this);

                foreach (var rule in textRules)
                {
                    if (rule != null && rule.Evaluate(context))
                    {
                        return InterpolateString(rule.text);
                    }
                }
            }

            return InterpolateString(text);
        }

        public ActLogicSave Save()
        {
            callTree.Root().Traverse(ct => ct.actID = ct.act != null ? ct.act.SaveID() : 0);
            var save = new ActLogicSave
            {
                fragSave = fragTree.Save(),
                callTree = callTree,
                activeAct = activeAct != null ? activeAct.SaveID() : 0,
                callbackAct = callbackAct != null ? callbackAct.SaveID() : 0,
                altAct = altAct != null ? altAct.SaveID() : 0,
                lastText = lastText,
                textHistory = textHistory
            };
            return save;
        }

        public void Load(ActLogicSave save)
        {
            fragTree.Load(save.fragSave);
            callTree = save.callTree;
            callTree.Root().Traverse(ct => ct.act = ct.actID.SavedObject<Act>());
            activeAct = save.activeAct.SavedObject<Act>();
            callbackAct = save.callbackAct.SavedObject<Act>();
            altAct = save.altAct.SavedObject<Act>();

            if (activeAct != null)
            {
                PopulateActList(activeAct.altActs, altActs, activeAct.randomAlt);
            }

            lastText = save.lastText;
            textHistory = save.textHistory;

            OnTextChange?.Invoke(textHistory);
        }

        private void OnCreateCard(CardViz cardViz)
        {
            cardViz.ParentTo(actWindow.transform, true);
        }

        private void Awake()
        {
            actWindow = GetComponent<ActWindow>();
            fragTree = GetComponent<FragTree>();
            callTree  = new();
        }

        public void OnEnable()
        {
            fragTree.onCreateCard += OnCreateCard;
        }

        public void OnDisable()
        {
            fragTree.onCreateCard -= OnCreateCard;
        }
    }

    [Serializable]
    public class CallTree
    {
        public Act act;
        public uint actID;
        [SerializeReference] public CallTree parent;
        [SerializeReference] public List<CallTree> children = new();

        public void Traverse(Action<CallTree> a)
        {
            a(this);
            foreach (var child in children)
            {
                child.Traverse(a);
            }
        }

        public CallTree Root()
        {
            var root = this;
            while (root.parent != null) root = root.parent;
            return root;
        }
    }

    [Serializable]
    public class ActText
    {
        public string label;
        public string text;
    }

    [Serializable]
    public class ActLogicSave
    {
        public FragTreeSave fragSave;
        public CallTree callTree;
        public uint activeAct;
        public uint callbackAct;

        public uint altAct;

        public ActText lastText;
        public List<ActText> textHistory;
    }

}
