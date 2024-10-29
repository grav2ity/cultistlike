using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

using TMPro;


namespace CultistLike
{
    public class ActWindow : Drag
    {
        [Header("Layout")]
        [SerializeField] private TextMeshPro text;
        [SerializeField] private GameObject idleSlotsGO;
        [SerializeField] private GameObject runSlotsGO;
        [SerializeField] private CardLane resultLane;
        [SerializeField] private FragmentBar fragmentBar;
        [SerializeField] private GameObject timerGO;
        [SerializeField] private Button okButton;
        [SerializeField] private Button collectButton;
        public Timer timer;

        [Header("Slots")]
        [SerializeField] private List<SlotViz> idleSlots;
        [SerializeField] private List<SlotViz> runSlots;

        [SerializeField, HideInInspector] private ActStatus actStatus;
        [SerializeField, HideInInspector] private Act readyAct;

        private TokenViz _tokenViz;
        private ActLogic actLogic;

        private bool suspendUpdates;

        public TokenViz tokenViz { get => _tokenViz; private set => _tokenViz = value; }

        private List<SlotViz> slots => actStatus == ActStatus.Running ? runSlots : idleSlots;
        private string runText => actLogic.altAct ? actLogic.altAct.text : actLogic.activeAct.text;


        public void TrySlotAndBringUp(CardViz cardViz)
        {
            if (actStatus != ActStatus.Finished)
            {
                foreach (var slot in slots)
                {
                    if (slot.TrySlotCard(cardViz) == true)
                    {
                        BringUp();
                        return;
                    }
                }
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
            actLogic.RunAct(readyAct);
        }

        public void FirstSlotEmpty()
        {
            suspendUpdates = true;
            ReturnCardsToTable();
            suspendUpdates = false;
            if (actStatus != ActStatus.Running)
            {
                StatusIdle();
            }
        }

        public void ParentCardsToWindow()
        {
            foreach (var slot in slots)
            {
                slot.ParentCardToWindow();
            }
        }

        //TODO has side effects
        public bool MatchesAnyOpenSlot(CardViz card) => HighlightSlots(card, false) == true;

        //TODO
        public bool HighlightSlots(CardViz cardViz, bool p = true)
        {
            bool highlighted = false;
            if (actStatus != ActStatus.Finished)
            {
                foreach (var slot in slots)
                {
                    if (slot.gameObject.activeSelf == true && slot.AcceptsCard(cardViz))
                    {
                        slot.SetHighlight(p);
                        highlighted = true;
                    }
                }
            }
            return highlighted;
        }

        //TODO
        public void UnhighlightSlots()
        {
            foreach (var slot in slots)
            {
                if (slot.gameObject.activeSelf)
                {
                    slot.SetHighlight(false);
                }
            }
        }

        public void HoldCard(CardViz cardViz)
        {
            if (cardViz != null)
            {
                actLogic.HoldCard(cardViz);
                UpdateSlots();
                if (actStatus == ActStatus.Running)
                {
                    ApplyStatus(ActStatus.Running);
                }
            }
        }

        public CardViz UnholdCard(CardViz cardViz)
        {
            if (cardViz != null)
            {
                var r = actLogic.UnholdCard(cardViz);
                UpdateSlots();
                if (actStatus == ActStatus.Running)
                {
                    ApplyStatus(ActStatus.Running);
                }
                return r;
            }

            return null;
        }

        public void UpdateSlots()
        {
            if (suspendUpdates == false)
            {
                suspendUpdates = true;

                CloseSlots(slots);

                var slotsToOpen = actLogic.CheckForSlots();
                foreach (var slot in slotsToOpen)
                {
                    OpenSlot(slot, slots);
                }

                bool reUpdate = false;
                foreach (var slot in slots)
                {
                    if (slot.gameObject.activeSelf == false)
                    {
                        var cardViz = slot.UnslotCard();
                        if (cardViz != null)
                        {
                            reUpdate = true;
                            GameManager.Instance.table.ReturnToTable(cardViz);
                        }
                    }
                }

                suspendUpdates = false;
                if (reUpdate == true)
                {
                    UpdateSlots();
                }
                UpdateBars();
            }
        }

        private void CloseSlots(List<SlotViz> slots)
        {
            foreach (var slotViz in slots)
            {
                slotViz.gameObject.SetActive(false);
            }
        }

        private void OpenSlot(Slot slot, List<SlotViz> slots)
        {
            foreach (var slotViz in slots)
            {
                if (slotViz.gameObject.activeSelf == false)
                {
                    slotViz.LoadSlot(slot);
                    slotViz.gameObject.SetActive(true);
                    break;
                }
            }
        }

        public void SetupResultCards(List<CardViz> cards)
        {
            resultLane.PlaceCards(cards);
            tokenViz.SetResultCount(cards.Count);

            ApplyStatus(ActStatus.Finished, actLogic.activeAct.endText);
        }

        /// <summary>
        /// Check for new Status after card slot/unslot.
        /// </summary>
        public void Check()
        {
            switch (actStatus)
            {
                case ActStatus.Idle:
                case ActStatus.Ready:
                    AttemptReadyAct();
                    break;
                case ActStatus.Finished:
                    var count = GetResultCards().Count;
                    if (count == 0)
                    {
                        StatusIdle();
                        if (tokenViz.token.dissolve == true)
                        {
                            tokenViz.Dissolve();
                            Close();
                            Destroy(this, 1f);
                        }
                    }
                    else
                    {
                        tokenViz.SetResultCount(count);
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

            GameManager.Instance.table.Place(tokenViz, l);
            resultLane.cards.Clear();

            Check();
            UpdateBars();
        }

        public void LoadToken(TokenViz tokenViz)
        {
            if (tokenViz != null)
            {
                this.tokenViz = tokenViz;
            }
        }

        public void AttemptReadyAct()
        {
            foreach (var act in GameManager.Instance.GetInitialActs())
            {
                if (act.token == null || act.token != tokenViz.token)
                {
                    continue;
                }
                readyAct = actLogic.AttemptAct(act);
                if (readyAct != null)
                {
                    break;
                }
            }

            if (readyAct != null)
            {
                ApplyStatus(ActStatus.Ready, readyAct.text);
            }
            else
            {
                ApplyStatus(ActStatus.Idle);
            }
        }

        public void Grab()
        {
            foreach (var slot in slots)
            {
                if (slot.gameObject.activeSelf == true && slot.grab == true)
                {
                    foreach (var cardViz in GameManager.Instance.cards)
                    {
                        if (cardViz.gameObject.activeSelf == false)
                            continue;

                        if (slot.AcceptsCard(cardViz) == true)
                        {
                            var cardVizY = cardViz.Yield();

                            if (cardVizY.free == false)
                                continue;

                            cardVizY.free = false;
                            cardVizY.transform.DOComplete(true);
                            bool prevInteractive = cardVizY.interactive;
                            cardVizY.interactive = false;

                            cardVizY.transform.parent?.GetComponentInParent<ICardDock>(true)?.
                                OnCardUndock(cardVizY.gameObject);
                            cardVizY.gameObject.SetActive(true);
                            cardVizY.transform.SetParent(null);
                            slot.SlotCardLogical(cardVizY);
                            cardVizY.transform.DOMove(tokenViz.transform.position, GameManager.Instance.normalSpeed).
                                OnComplete(() => { cardVizY.interactive = prevInteractive; slot.SlotCardPhysical(cardVizY); });

                            return;
                        }
                    }
                }
            }
        }

        private List<CardViz> GetResultCards()
        {
            return resultLane.cards;
        }

        private void StatusIdle()
        {
            actLogic.Reset();
            UpdateSlots();

            ApplyStatus(ActStatus.Idle);
        }

        public void UpdateBars()
        {
            if (suspendUpdates == false)
            {
                fragmentBar.Load(actLogic.fragments);
            }
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
                    idleSlotsGO.SetActive(true);
                    runSlotsGO.SetActive(false);
                    resultLane.gameObject.SetActive(false);
                    okButton.interactable = false;
                    collectButton.interactable = false;
                    tokenViz.SetResultCount(0);
                    if (tokenViz != null)
                    {
                        text.text = tokenViz?.token?.description;
                    }
                    break;
                case ActStatus.Ready:
                    okButton.interactable = true;
                    text.text = tex;
                    break;
                case ActStatus.Running:
                    timerGO.SetActive(true);
                    idleSlotsGO.SetActive(false);
                    runSlotsGO.SetActive(true);
                    okButton.interactable = false;
                    text.text = runText;
                    break;
                case ActStatus.Finished:
                    timerGO.SetActive(false);
                    runSlotsGO.SetActive(false);
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
            foreach (var cardSlot in idleSlots)
            {
                if (cardSlot.slottedCard != null)
                {
                    var cardViz = cardSlot.UnslotCard();
                    GameManager.Instance.table.ReturnToTable(cardViz);
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

            timerGO.SetActive(false);
            collectButton.interactable = false;
            tokenViz.SetResultCount(0);

            gameObject.SetActive(false);

            CloseSlots(idleSlots);
            CloseSlots(runSlots);
            StatusIdle();

            foreach(var c in gameObject.GetComponentsInChildren<Canvas>())
            {
                c.worldCamera = Camera.main;
            }

            if (tokenViz.autoPlay != null)
            {
                readyAct = tokenViz.autoPlay;
                GoForIt();
            }
        }
    }

    public enum ActStatus
    {
            Idle,
            Ready,
            Running,
            Finished
    }
}
