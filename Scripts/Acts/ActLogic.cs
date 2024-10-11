using System.Collections.Generic;

using UnityEngine;


namespace CultistLike
{
    public class ActLogic : MonoBehaviour
    {
        [SerializeField] private List<CardViz> heldCards;
        [SerializeField] private List<AspectViz> heldAspects;

        [SerializeField, HideInInspector] private List<Rule> _rules;
        [SerializeField, HideInInspector] private Act _activeAct;
        [SerializeField, HideInInspector] private bool _isAct;

        private ActWindow actWindow;


        public List<Rule> rules { get => _rules; private set => _rules = value; }
        public Act activeAct { get => _activeAct; private set => _activeAct = value; }
        public bool isAct { get => _isAct; private set => _isAct = value; }

        private ActViz actViz { get => actWindow.actViz; }


        public void RunAct(Act act)
        {
            activeAct = act;
            isAct = true;

            rules.Clear();
            rules = activeAct.rules.GetRange(0, activeAct.rules.Count);

            //TODO do not always show slot
            if (rules.Count > 0)
            {
                actWindow.OpenExtraSlot(activeAct.slotTitle, activeAct.cardLock);
            }

            actWindow.ApplyStatus(ActStatus.Running);

            if (activeAct.grab == true)
            {
                actWindow.Grab();
            }

            //in order to update window status
            actWindow.CheckForReady(true);

            if (act.time > 0)
            {
                actViz.timer.StartTimer(act.time, () =>
                {
                    actViz.ShowTimer(false);
                    SetupActResults();
                });
                actViz.ShowTimer(true);
            }
            else
            {
                SetupActResults();
            }
        }

        public void RunRule(Rule rule)
        {
            if (rule == null)
            {
                return;
            }

            isAct = false;

            actWindow.CloseExtraSlot();

            actWindow.ApplyStatus(ActStatus.Running);

            if (rule.time > 0)
            {
                actViz.timer.StartTimer(rule.time, () =>
                {
                    actViz.ShowTimer(false);
                    SetupRuleResults(rule);
                });
                actViz.ShowTimer(true);
            }
            else
            {
                SetupRuleResults(rule);
            }
        }

        public void Init(Act act)
        {
            activeAct = act;
        }

        public void Reset()
        {
            rules.Clear();
            isAct = false;

            heldCards.Clear();
            heldAspects.Clear();
        }

        private void SetupActResults()
        {
            var rule = actWindow.CheckForReady(true);
            if (rule != null)
            {
                RunRule(rule);
            }
            else
            {
                SetupFinalResults("ACT END TEXT");
            }
        }

        private void SetupRuleResults(Rule rule)
        {
            Result result = rule.GenerateResults();

            foreach (var cardViz in heldCards)
            {
                cardViz.gameObject.SetActive(false);
                cardViz.transform.SetParent(transform);
            }

            ApplyModifiers(result);

            if (result.nextAct != null)
            {
                RunAct(result.nextAct);
            }
            else
            {
                var endText = result.endText != "" ? result.endText : rule.endText;
                SetupFinalResults(endText);
            }
        }

        private void SetupFinalResults(string endText)
        {
            if (heldCards != null)
            {
                actWindow.SetupResultCards(heldCards);
                heldCards.Clear();
            }

            actWindow.ApplyStatus(ActStatus.Finished, endText);

            activeAct = actViz.act;
            isAct = true;
        }

        private void ApplyModifiers(Result result)
        {
            foreach (var actModifier in result.actModifiers)
            {
                actModifier.Apply(this);
            }
            foreach (var tableModifier in result.tableModifiers)
            {
                tableModifier.Apply(actViz);
            }
        }


        public void HoldCard(CardViz cardViz)
        {
            if (cardViz != null)
            {
                heldCards.Add(cardViz);
            }
        }

        public void HoldCard(Card card)
        {
            if (card != null)
            {
                var cardViz = Instantiate(GameManager.Instance.cardPrefab);
                cardViz.SetCard(card);
                HoldCard(cardViz);
            }
        }

        public CardViz UnholdCard(CardViz cardViz)
        {
            if (cardViz != null)
            {
                heldCards.Remove(cardViz);
            }
            return cardViz;
        }

        public CardViz UnholdCard(Card card)
        {
            if (card != null)
            {
                var cardViz = heldCards.Find(x => x.card == card);
                if (cardViz != null)
                {
                    UnholdCard(cardViz);
                }
                return cardViz;
            }
            return null;
        }

        public CardViz UnholdCard(Requirement requirement)
        {
            var cardViz = heldCards.Find(cardViz => requirement.AttemptOne(cardViz.card) == true );
            if (cardViz != null)
            {
                UnholdCard(cardViz);
                return cardViz;
            }
            else
            {
                return null;
            }
        }

        public void HoldAspect(AspectViz aspectViz)
        {
            if (aspectViz != null)
            {
                int i = heldAspects.IndexOf(aspectViz);
                if (i != -1)
                {
                    heldAspects[i].count = heldAspects[i].count + aspectViz.count;
                }
                else
                {
                    heldAspects.Add(aspectViz);
                }
            }
        }

        public void HoldAspect(Aspect aspect)
        {
            if (aspect != null)
            {
                var aspectViz = heldAspects.Find(x => x.aspect == aspect);
                if (aspectViz != null)
                {
                    aspectViz.count = aspectViz.count + 1;
                }
                else
                {
                    aspectViz = Instantiate(GameManager.Instance.aspectPrefab, transform);
                    aspectViz.LoadAspect(aspect);
                    aspectViz.gameObject.SetActive(false);
                    heldAspects.Add(aspectViz);
                }
            }
        }

        public void HoldAspect(Act act)
        {
            if (act != null)
            {
                foreach (var aspect in act.aspects)
                {
                    HoldAspect(aspect);
                }
            }
        }

        private void Awake()
        {
            actWindow = GetComponent<ActWindow>();

            heldCards = new List<CardViz>();
            heldAspects = new List<AspectViz>();
        }
    }
}
