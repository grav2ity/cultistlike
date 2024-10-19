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

        private ActWindow actWindow;



        public FragContainer fragments { get => _fragments; private set => _fragments = value; }
        public Act activeAct { get => _activeAct; private set => _activeAct = value; }
        public Act altAct { get => _altAct; set => _altAct = value; }

        private TokenViz tokenViz { get => actWindow.tokenViz; }


        public List<Slot> CheckForSlots()
        {
            List<Slot> slotsToOpen = new List<Slot>();

            if (activeAct != null)
            {
                foreach (var slot in activeAct.slots)
                {
                    if (slot.Opens(fragments) == true)
                    {
                        slotsToOpen.Add(slot);
                    }
                }

                if (activeAct.spawnGlobalSlots == true)
                {
                    foreach (var slot in GameManager.Instance.slotTypes)
                    {
                        if (slot.Opens(fragments) == true)
                        {
                            slotsToOpen.Add(slot);
                        }
                    }
                }
            }
            else
            {
                slotsToOpen.Add(actWindow.tokenViz?.token?.slot);
            }
            return slotsToOpen;
        }


        public void RunAct(Act act)
        {
            Debug.Log("Running act: " + act.name);
            activeAct = act;

            foreach (var aspect in activeAct.aspects)
            {
                fragments.Add(aspect);
            }

            actWindow.UpdateBars();
            actWindow.ParentCardsToWindow();
            //need this for correct slots
            actWindow.ApplyStatus(ActStatus.Running);
            actWindow.UpdateSlots();
            actWindow.Grab();

            PopulateActLists(act.altActs, altActs);

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

        private void PopulateActLists(List<ActLink> source, List<Act> target)
        {
            if (source != null && target != null)
            {
                target.Clear();

                if (source.Count == 1 && source[0].chance == 0)
                {
                    target.Add(source[0].act);
                }
                else
                {
                    foreach (var actLink in source)
                    {
                        int r = (int)(100f * Random.Range(0.0f, 1.0f));
                        if (r < actLink.chance)
                        {
                            target.Add(actLink.act);
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

            ApplyModifiers(activeAct);

            actWindow.ParentCardsToWindow();
            actWindow.UpdateBars();

            foreach (var cardViz in fragments.cards)
            {
                cardViz.gameObject.SetActive(false);
                cardViz.transform.SetParent(transform);
            }

            PopulateActLists(activeAct.nextActs, nextActs);
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

        private void SetupFinalResults(string endText)
        {
            if (fragments.cards != null)
            {
                actWindow.SetupResultCards(fragments.cards);
                fragments.cards.Clear();
            }

            activeAct = null;
        }

        public Act AttemptAct(Act act)
        {
            if (act != null)
            {
                if (act.token != null && act.token != actWindow.tokenViz.token)
                {
                    return null;
                }
                if (act.Attempt(fragments) == true)
                {
                    return act;
                }
            }
            return null;
        }

        public Act AttemptActs(List<Act> acts)
        {
            foreach (var act in acts)
            {
                if (act != null && act.Attempt(fragments) == true)
                {
                    return act;
                }
            }
            return null;
        }


        public Act AttemptAltActs() => AttemptActs(altActs);
        public Act AttemptNextActs() => AttemptActs(nextActs);


        private void ApplyModifiers(Act act)
        {
            foreach (var actModifier in act.actModifiers)
            {
                actModifier.Apply(this);
            }
            foreach (var tableModifier in act.tableModifiers)
            {
                tableModifier.Apply(tokenViz);
            }

            foreach (var rule in act.rules)
            {
                ApplyModifiers(rule);
            }
        }

        private void ApplyModifiers(Rule rule)
        {
            foreach (var actModifier in rule.actModifiers)
            {
                actModifier.Apply(this);
            }
            foreach (var tableModifier in rule.tableModifiers)
            {
                tableModifier.Apply(tokenViz);
            }
        }


        public void HoldCard(CardViz cardViz)
        {
            if (cardViz != null)
            {
                fragments.Add(cardViz);
                actWindow.UpdateBars();
                altAct = AttemptAltActs();
            }
        }

        public CardViz UnholdCard(CardViz cardViz)
        {
            if (cardViz != null)
            {
                fragments.Remove(cardViz);
                actWindow.UpdateBars();
                altAct = AttemptAltActs();
            }
            return cardViz;
        }

        public void HoldFragment(Fragment frag)
        {
            fragments.Add(frag);
            actWindow.UpdateBars();
            altAct = AttemptAltActs();
        }

        public void AdjustFragment(Fragment frag, int level)
        {
            fragments.Adjust(frag, level);
        }

        public void DestroyCard(CardViz cardViz)
        {
            fragments.DestroyCard(cardViz);
        }

        private void Awake()
        {
            actWindow = GetComponent<ActWindow>();
            altActs = new List<Act>();
            nextActs = new List<Act>();

            fragments = new FragContainer();
        }
    }
}
