using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using DG.Tweening;
using TMPro;


namespace CultistLike
{
    public class ActWindow : Drag, IDropHandler
    {
        [Header("Layout")]
        [SerializeField] private TextMeshPro text;
        [SerializeField] private GameObject cardSlotsGO;
        [SerializeField] private CardLane resultLane;
        [SerializeField] private GameObject timerGO;
        [SerializeField] private Button okButton;
        [SerializeField] private Button collectButton;
        public Timer timer;

        [Header("Slots")]
        [SerializeField] private List<Slot> cardSlots;

        [SerializeField, HideInInspector]
        private ActStatus actStatus  = ActStatus.Idle;

        [SerializeField, HideInInspector]
        private ActViz actViz;

        //Rule ready to be run with the slotted cards
        [SerializeField, HideInInspector]
        private Rule readyRule;
        //Rules initiated (set) by a card slotted in the first slot
        //that need additional cards to become ready
        [SerializeField, HideInInspector]
        private List<Rule> setRules;


        private enum ActStatus
        {
            Idle,
            Set,
            Ready,
            Running,
            Finished
        }

        private Slot firstSlot { get => cardSlots[0]; }
        private CardViz firstCardViz { get => cardSlots[0].slottedCard; }

        public void OnDrop(PointerEventData eventData) {}

        public void TrySlotAndBringUp(CardViz cardViz)
        {
            if (actStatus == ActStatus.Running || actStatus == ActStatus.Finished)
            {
                return;
            }
            else
            {
                //TODO slot in a slot with best matching reqs?
                bool foundEmpytSlot = false;
                foreach (Slot cardSlot in cardSlots)
                {
                    if (cardSlot.empty == true)
                    {
                        cardSlot.SlotCard(cardViz);
                        foundEmpytSlot = true;
                        break;
                    }
                }

                if (foundEmpytSlot == false)
                {
                    firstSlot.UnslotCard();
                    GameManager.Instance.table.ReturnToTable(firstCardViz);
                    firstSlot.SlotCard(cardViz);
                }

                BringUp();
            }
        }

        public void BringUp()
        {
            gameObject.SetActive(true);
            GameManager.Instance.OpenWindow(this);
        }

        public void Close()
        {
            switch (actStatus)
            {
                case ActStatus.Idle:
                case ActStatus.Set:
                case ActStatus.Ready:
                    ReturnCardsToTable();
                    StatusIdle();
                    break;
                default:
                    break;
            }

            gameObject.SetActive(false);
            GameManager.Instance.CloseWindow();
        }

        public void GoConsuming()
        {
            actViz.timer.StartTimer(actViz.act.consumeRule.time, () =>
            {
                actViz.Consume();
                GoConsuming();
            });
            actViz.ShowTimer(true);

            ApplyStatus(ActStatus.Running);
        }

        /// <summary>
        /// Run the ready rule.
        /// </summary>
        public void GoForIt()
        {
            if (readyRule == null)
            {
                return;
            }

            actViz.timer.StartTimer(readyRule.time, () =>
            {
                SetupResults();
                actViz.ShowTimer(false);
            });
            actViz.ShowTimer(true);

            DestroySlotted();
            ApplyStatus(ActStatus.Running);
        }

        public void HighlightAct(bool p)
        {
            actViz.SetHighlight(p);
        }

        /// <summary>
        /// Highlight cards accepted by the i-th slot.
        /// </summary>
        /// <param name="i">Indexed from 0.</param>
        public void HighlightCards(int i)
        {
            List<CardViz> cardsToH = new List<CardViz>();
            var cards = GameManager.Instance.table.GetCards();
            if (i == 0)
            {
                foreach (var cardViz in cards)
                {
                    if(actViz.act.AttemptFirst(cardViz.card).Count != 0)
                    {
                        cardsToH.Add(cardViz);
                    }
                }
            }
            else if (i > 0 && i < cardSlots.Count)
            {
                foreach (var rule in setRules)
                {
                    foreach (var cardViz in cards)
                    {
                        if (rule.AttemptOne(i, cardViz.card) == true)
                        {
                            cardsToH.Add(cardViz);
                        }
                    }
                }
            }
            GameManager.Instance.table.HighlightCards(cardsToH);
        }

        /// <summary>
        /// Set highlight status of slots that can accept given <c>card</c>.
        /// </summary>
        /// <param name="card"></param>
        /// <param name="p">Highlight status.</param>
        /// <returns>True if <c>card</c> would be accepted by any slot.</returns>
        public bool HighlightSlots(Card card, bool p = true)
        {
            bool highlighted = false;
            if (card != null)
            {
                switch (actStatus)
                {
                    case ActStatus.Idle:
                    case ActStatus.Set:
                    case ActStatus.Ready:
                        List<Rule> newRules = actViz.act.AttemptFirst(card);
                        if (newRules.Count != 0)
                        {
                            firstSlot.SetHighlight(p);
                            highlighted = true;
                        }
                        for (int i=1; i<cardSlots.Count; i++)
                        {
                            foreach (var rule in setRules)
                            {
                                bool b = rule.AttemptOne(i, card);
                                if (b == true)
                                {
                                    cardSlots[i].SetHighlight(p);
                                    highlighted = true;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                foreach (var slot in cardSlots)
                {
                    slot.SetHighlight(false);
                }
            }

            return highlighted;
        }


        /// <summary>
        /// Check for new Status after card slot/unslot.
        /// </summary>
        public void Check()
        {
            switch (actStatus)
            {
                case ActStatus.Idle:
                    if (firstCardViz == null)
                    {
                        return;
                    }
                    CheckForSet();
                    CheckForReady();
                    break;
                case ActStatus.Set:
                case ActStatus.Ready:
                    if (firstCardViz == null)
                    {
                        StatusIdle();
                        return;
                    }
                    CheckForSet();
                    if (setRules.Count == 0)
                    {
                        readyRule = null;
                        ApplyStatus(ActStatus.Idle);
                    }
                    else
                    {
                        CheckForReady();
                    }
                    break;
                case ActStatus.Running:
                    break;
                case ActStatus.Finished:
                    var count = GetResultCards().Count;
                    if (count == 0)
                    {
                        StatusIdle();
                    }
                    else
                    {
                        actViz.SetResultCount(count);
                    }
                    break;
                default:
                    break;
            }
        }

        public void CollectAll()
        {
            List<Viz> l = new List<Viz>();
            foreach (var cardViz in resultLane.cards)
            {
                l.Add(cardViz);
                cardViz.ShowFace();
            }

            GameManager.Instance.table.Place(actViz, l);
            resultLane.cards.Clear();

            Check();
        }

        public void LoadAct(ActViz actViz)
        {
            if (actViz != null)
            {
                this.actViz = actViz;
                text.text = actViz.act.text;
            }
        }


        private int CheckForSet()
        {
            setRules = actViz.act.AttemptFirst(firstCardViz.card);
            if (setRules.Count != 0)
            {
                ApplyStatus(ActStatus.Set);
            }
            return setRules.Count;
        }

        private bool CheckForReady()
        {
            foreach (var rule in setRules)
            {
                if (rule.Attempt(GetSlottedCards()) == true)
                {
                    ApplyStatus(ActStatus.Ready, rule.startText);
                    readyRule = rule;
                    return true;
                }
            }
            readyRule = null;
            return false;
        }

        private void SetupResults()
        {
            if (readyRule == null)
            {
                return;
            }

            Result result = readyRule.GenerateResults();

            if (result.cards != null)
            {
                List<CardViz> cards  = new List<CardViz>();
                foreach (Card card in result.cards)
                {
                    if (card == null)
                    {
                        Debug.LogWarning("Missing Results card in " + actViz.act.actName);
                    }

                    var cardViz = Instantiate(GameManager.Instance.cardPrefab);
                    cardViz.SetCard(card);
                    cards.Add(cardViz);
                }

                resultLane.PlaceCards(cards);

                actViz.SetResultCount(result.cards.Count);
            }

            ApplyStatus(ActStatus.Finished,
                        result.endText != "" ? result.endText : readyRule.endText);

            if (result.extra != null)
            {
                Act act = result.extra as Act;
                if (act != null)
                {
                    var newActViz = Instantiate(GameManager.Instance.actPrefab,
                                                actViz.transform.position, Quaternion.identity);
                    newActViz.SetAct(act);

                    var root = newActViz.transform;
                    var localScale = root.localScale;

                    GameManager.Instance.table.Place(actViz, new List<Viz> { newActViz });

                    root.localScale = new Vector3(0f, 0f, 0f);
                    root.DOScale(localScale, 1);
                }
            }
        }

        private List<Card> GetSlottedCards()
        {
            List<Card> cards = new List<Card>();
            foreach (var cardSlot in cardSlots)
            {
                if (cardSlot.slottedCard != null)
                {
                    cards.Add(cardSlot.slottedCard.card);
                }
            }
            return cards;
        }

        public List<CardViz> GetResultCards()
        {
            return resultLane.cards;
        }

        private void DestroySlotted()
        {
            foreach (var cardSlot in cardSlots)
            {
                cardSlot.DestroyCard();
            }
        }

        private void StatusIdle()
        {
            readyRule = null;
            setRules.Clear();
            ApplyStatus(ActStatus.Idle);
        }

        /// <summary>
        /// Update visuals to reflect status.
        /// </summary>
        /// <param name="actStatus"></param>
        /// <param name="tex"></param>
        private void ApplyStatus(ActStatus actStatus, string tex = "")
        {
            this.actStatus = actStatus;
            switch (actStatus)
            {
                case ActStatus.Idle:
                    cardSlotsGO.SetActive(true);
                    resultLane.gameObject.SetActive(false);
                    okButton.interactable = false;
                    collectButton.interactable = false;
                    actViz.SetResultCount(0);
                    for (int i=1; i<cardSlots.Count; i++)
                    {
                        cardSlots[i].CloseSlot();
                    }
                    firstSlot.OpenSlot();
                    if (actViz != null)
                    {
                        text.text = actViz.act.text;
                    }
                    break;
                case ActStatus.Set:
                    okButton.interactable = false;
                    var maxSlots = 0;
                    foreach (var rule in setRules)
                    {
                        for (int j=0; j<rule.requirements.Count; j++)
                        {
                            if (j>maxSlots)
                            {
                                maxSlots = j;
                                cardSlots[j].OpenSlot();
                            }
                            if (rule.requirements[j].name != "")
                            {
                                cardSlots[j].Title = rule.requirements[j].name;
                            }
                        }
                    }
                    text.text = tex;
                    break;
                case ActStatus.Ready:
                    okButton.interactable = true;
                    text.text = tex;
                    break;
                case ActStatus.Running:
                    timerGO.SetActive(true);
                    cardSlotsGO.SetActive(false);
                    okButton.interactable = false;
                    break;
                case ActStatus.Finished:
                    timerGO.SetActive(false);
                    resultLane.gameObject.SetActive(true);
                    collectButton.interactable = true;
                    text.text = tex;
                    break;
                default:
                    break;
            }
        }

        private void ReturnCardsToTable()
        {
            foreach (var cardSlot in cardSlots)
            {
                if (cardSlot.slottedCard != null)
                {
                    GameManager.Instance.table.ReturnToTable(cardSlot.slottedCard);
                    cardSlot.UnslotCard();
                }
            }
        }

        private void Start()
        {
            GetComponent<Drag>().draggingPlane = GameManager.Instance.windowPlane;

            firstSlot.Title = actViz.act.actName;
            timerGO.SetActive(false);
            collectButton.interactable = false;
            actViz.SetResultCount(0);

            gameObject.SetActive(false);

            if (actViz.act.consumeRule == null)
            {
                ApplyStatus(ActStatus.Idle);
            }
            else
            {
                GoConsuming();
            }

            for (int i=0; i<cardSlots.Count; i++)
            {
                cardSlots[i].index = i;
            }

            foreach(var c in gameObject.GetComponentsInChildren<Canvas>())
            {
                c.worldCamera = Camera.main;
            }
        }
    }
}
