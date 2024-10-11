using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using DG.Tweening;
using TMPro;


namespace CultistLike
{
    public class ActWindow : Drag
    {
        [Header("Layout")]
        [SerializeField] private TextMeshPro text;
        [SerializeField] private GameObject cardSlotsGO;
        [SerializeField] private Slot extraSlot;
        [SerializeField] private CardLane resultLane;
        [SerializeField] private GameObject timerGO;
        [SerializeField] private Button okButton;
        [SerializeField] private Button collectButton;
        public Timer timer;

        [Header("Slots")]
        [SerializeField] private List<Slot> cardSlots;

        [SerializeField, HideInInspector] private ActStatus actStatus;
        [SerializeField, HideInInspector] private Rule readyRule;

        [SerializeField, HideInInspector] private List<Rule> initialRules;

        private ActViz _actViz;
        private ActLogic actLogic;


        public ActViz actViz { get => _actViz; private set => _actViz = value; }

        private Act activeAct { get => actLogic.activeAct; }
        private bool isAct { get => actLogic.isAct; }

        private List<Rule> setRules
        {
            get => actStatus == ActStatus.Running ? actLogic.rules : initialRules;
        }

        private string runText
        {
            get
            {
                if (isAct == true)
                {
                    return readyRule ? readyRule.startText : activeAct.text;
                }
                else
                {
                    return readyRule.runText;
                }
            }
        }

        private Slot firstSlot { get => cardSlots[0]; }
        private CardViz firstCardViz { get => cardSlots[0].slottedCard; }


        public void TrySlotAndBringUp(CardViz cardViz)
        {
            if (actStatus == ActStatus.Finished)
            {
                return;
            }
            else if (actStatus == ActStatus.Running)
            {
                if (activeAct.cardLock == true)
                {
                    return;
                }
                if (extraSlot.gameObject.activeSelf == true)
                {
                    if (CheckExtra(cardViz.card) == true)
                    {
                        if (extraSlot.empty == true)
                        {
                            extraSlot.SlotCard(cardViz);
                        }
                        else
                        {
                            var sc = extraSlot.UnslotCard();
                            GameManager.Instance.table.ReturnToTable(sc);
                            extraSlot.SlotCard(cardViz);
                        }
                        BringUp();
                    }
                }
                else
                {
                    return;
                }
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
                    var prevCardViz = firstSlot.UnslotCard();
                    GameManager.Instance.table.ReturnToTable(prevCardViz);
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

        /// <summary>
        /// Run the ready rule.
        /// </summary>
        public void GoForIt()
        {
            if (readyRule != null)
            {
                actLogic.RunRule(readyRule);

                for (int i=0; i<cardSlots.Count; i++)
                {
                    cardSlots[i].MoveCardToAct();
                }
            }
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
                    if(activeAct.AttemptFirst(cardViz.card).Count != 0)
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

        //TODO has side effects
        public bool MatchesAnyOpenSlot(Card card) => HighlightSlots(card, false) == true;

        /// <summary>
        /// Set highlight status of slots that can accept given <c>card</c>.
        /// </summary>
        /// <param name="card"></param>
        /// <param name="p">Highlight status.</param>
        /// <returns>True if <c>card</c> would be accepted by any slot.</returns>
        public bool HighlightSlots(Card card, bool p = true)
        {
            bool highlighted = false;
            if (card != null &&
                !(isAct == true && activeAct.grab == true && activeAct.cardLock == true))
            {
                switch (actStatus)
                {
                    case ActStatus.Idle:
                    case ActStatus.Set:
                    case ActStatus.Ready:
                        List<Rule> newRules = activeAct.AttemptFirst(card);
                        if (newRules.Count != 0)
                        {
                            firstSlot.SetHighlight(p);
                            highlighted = true;
                        }
                        for (int i=1; i<cardSlots.Count; i++)
                        {
                            if (cardSlots[i].gameObject.activeSelf == false)
                            {
                                continue;
                            }
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
                    case ActStatus.Running:
                        foreach (var rule in setRules)
                        {
                            bool b = rule.AttemptOne(0, card);
                            if (b == true)
                            {
                                extraSlot.SetHighlight(p);
                                highlighted = true;
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
                    extraSlot.SetHighlight(false);
                }
            }

            return highlighted;
        }

        public void HoldCard(CardViz cardViz)
        {
            actLogic.HoldCard(cardViz);
        }

        public CardViz UnholdCard(CardViz cardViz)
        {
            return actLogic.UnholdCard(cardViz);
        }

        public void OpenExtraSlot(string title, bool cardLock)
        {
            extraSlot.OpenSlot();
            extraSlot.Title = title;
            extraSlot.cardLock = cardLock;
        }

        public void CloseExtraSlot()
        {
            extraSlot.gameObject.SetActive(false);
        }

        public void SetupResultCards(List<CardViz> cards)
        {
            resultLane.PlaceCards(cards);
            actViz.SetResultCount(cards.Count);
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
                    CheckForReady(true);
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
                actLogic.Init(actViz.act);

                text.text = actViz.act.text;
            }
        }

        public Rule CheckForReady(bool running = false)
        {
            int cardsUsed = -1;
            Rule bestRule = null;

            foreach (var rule in setRules)
            {
                if (rule == null)
                {
                    continue;
                }
                if (rule.Attempt(GetSlottedCards()) == true)
                {
                    if (rule.requirements.Count > cardsUsed)
                    {
                        cardsUsed = rule.requirements.Count;
                        bestRule = rule;
                    }
                }
            }
            if (bestRule != null)
            {
                readyRule = bestRule;
                if (running)
                {
                    ApplyStatus(ActStatus.Running);
                }
                else
                {
                    ApplyStatus(ActStatus.Ready, bestRule.startText);
                }

                return readyRule;
            }
            else
            {
                readyRule = null;
                return null;
            }
        }

        private int CheckForSet()
        {
            initialRules = activeAct.AttemptFirst(firstCardViz.card);
            if (initialRules.Count != 0)
            {
                ApplyStatus(ActStatus.Set);
            }
            return initialRules.Count;
        }

        private bool CheckExtra(Card card)
        {
            foreach (var rule in setRules)
            {
                if (rule.requirements.Count > 0 && rule.Attempt(new List<Card>{card}) == true)
                {
                    return true;
                }
            }
            return false;
        }

        public void Grab()
        {
            foreach (var cardViz in GameManager.Instance.GetCards())
            {
                foreach (var rule in setRules)
                {
                    if (rule.AttemptFirst(cardViz.card) == true)
                    {
                        var cardVizY = cardViz.Yield();

                        //TODO
                        cardVizY.transform.DOComplete(true);
                        bool prevInteractive = cardVizY.interactive;
                        cardVizY.interactive = false;

                        cardVizY.transform.parent?.GetComponentInParent<ICardDock>(true)?.
                            OnCardUndock(cardVizY.gameObject);
                        cardVizY.gameObject.SetActive(true);
                        cardVizY.transform.SetParent(null);
                        extraSlot.slottedCard = cardVizY;
                        cardVizY.transform.DOMove(actViz.transform.position, GameManager.Instance.normalSpeed).
                            OnComplete(() => { cardVizY.interactive = prevInteractive; extraSlot.SlotCard(cardVizY, true); });
                        return;
                    }
                }
            }
        }

        public List<Card> GetSlottedCards()
        {
            List<Card> cards = new List<Card>();
            if (cardSlotsGO.activeSelf == true)
            {
                foreach (var cardSlot in cardSlots)
                {
                    if (cardSlot.slottedCard != null)
                    {
                        cards.Add(cardSlot.slottedCard.card);
                    }
                }
            }
            else if (extraSlot.gameObject.activeSelf == true)
            {
                if (extraSlot.slottedCard != null)
                {
                    cards.Add(extraSlot.slottedCard.card);
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
            extraSlot.DestroyCard();
        }

        private void StatusIdle()
        {
            actLogic.Reset();
            readyRule = null;

            ApplyStatus(ActStatus.Idle);
        }

        /// <summary>
        /// </summary>
        /// <param name="actStatus"></param>
        /// <param name="tex"></param>
        public void ApplyStatus(ActStatus actStatus, string tex = "")
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
                    extraSlot.CloseSlot();
                    firstSlot.OpenSlot();
                    if (actViz != null)
                    {
                        text.text = activeAct.text;
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
                    text.text = runText;
                    break;
                case ActStatus.Finished:
                    timerGO.SetActive(false);
                    extraSlot.gameObject.SetActive(false);
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

        private void Awake()
        {
            actLogic = GetComponent<ActLogic>();
        }

        private void Start()
        {
            GetComponent<Drag>().draggingPlane = GameManager.Instance.windowPlane;

            firstSlot.Title = actViz.act.actName;
            timerGO.SetActive(false);
            collectButton.interactable = false;
            actViz.SetResultCount(0);

            gameObject.SetActive(false);

            StatusIdle();

            for (int i=0; i<cardSlots.Count; i++)
            {
                cardSlots[i].index = i;
            }
            extraSlot.index = 0;

            foreach(var c in gameObject.GetComponentsInChildren<Canvas>())
            {
                c.worldCamera = Camera.main;
            }

            if (activeAct.autoPlay == true)
            {
                actLogic.RunAct(activeAct);
            }
        }
    }

    public enum ActStatus
    {
            Idle,
            Set,
            Ready,
            Running,
            Finished
    }
}
