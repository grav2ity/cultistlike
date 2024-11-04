using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class ActWindow : Drag
    {
        [Header("Layout")]
        [SerializeField] private TextMeshPro label;
        [SerializeField] private TextMeshPro text;
        [SerializeField] private GameObject idleSlotsGO;
        [SerializeField] private GameObject runSlotsGO;
        [SerializeField] private CardLane resultLane;
        [SerializeField] private FragmentBar aspectBar;
        [SerializeField] private FragmentBar cardBar;
        [SerializeField] private Timer _timer;
        [SerializeField] private Button okButton;
        [SerializeField] private Button collectButton;

        [Header("Slots")]
        [SerializeField] private List<SlotViz> idleSlots;
        [SerializeField] private List<SlotViz> runSlots;

        [SerializeField, HideInInspector] private ActStatus actStatus;
        [SerializeField, HideInInspector] private Act readyAct;

        private TokenViz _tokenViz;
        private ActLogic actLogic;

        private bool suspendUpdates;


        public TokenViz tokenViz { get => _tokenViz; private set => _tokenViz = value; }
        public Timer timer { get => _timer; }

        private List<SlotViz> slots => actStatus == ActStatus.Running ? runSlots : idleSlots;
        private string runText => actLogic.altAct ? actLogic.altAct.text : actLogic.activeAct.text;
        private string runLabel => actLogic.altAct ? actLogic.altAct.label : actLogic.activeAct.label;


        public bool TrySlotAndBringUp(CardViz cardViz)
        {
            if (actStatus != ActStatus.Finished)
            {
                var slotViz = AcceptsCard(cardViz);
                if (slotViz != null && slotViz.TrySlotCard(cardViz) == true)
                {
                    BringUp();
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<SlotViz> MatchingSlots(CardViz cardViz, bool onlyEmpty = false)
        {
            if (cardViz != null)
            {
                if (actStatus != ActStatus.Finished)
                {
                    foreach (var slotViz in slots)
                    {
                        if (slotViz.gameObject.activeSelf == true && slotViz.AcceptsCard(cardViz) == true)
                        {
                            if (onlyEmpty == false || slotViz.slottedCard == null)
                            {
                                yield return slotViz;
                            }
                        }
                    }
                }
            }
            else
            {
                yield return null;
            }
        }

        public SlotViz AcceptsCard(CardViz cardViz, bool onlyEmpty = false)
        {
            foreach (var slotViz in MatchingSlots(cardViz, onlyEmpty))
            {
                if (slotViz != null)
                {
                    return slotViz;
                }
            }
            return null;
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
            if (suspendUpdates == false)
            {
                suspendUpdates = true;
                ReturnCardsToTable();
                suspendUpdates = false;
                if (actStatus != ActStatus.Running)
                {
                    StatusIdle();
                }
            }
        }

        public void ParentCardsToWindow()
        {
            foreach (var slot in slots)
            {
                slot.ParentCardToWindow();
            }
        }

        public void HighlightSlots(CardViz cardViz, bool p = true)
        {
            foreach (var slotViz in MatchingSlots(cardViz))
            {
                slotViz.SetHighlight(p);
            }
        }

        public void UnhighlightSlots()
        {
            foreach (var slotViz in slots)
            {
                if (slotViz.gameObject.activeSelf)
                {
                    slotViz.SetHighlight(false);
                }
            }
        }

        public void HoldCard(CardViz cardViz, Slot slot = null)
        {
            if (cardViz != null)
            {
                actLogic.HoldCard(cardViz, slot);
                UpdateSlots();
                if (actStatus == ActStatus.Running)
                {
                    ApplyStatus(ActStatus.Running);
                }
            }
        }

        public CardViz UnholdCard(CardViz cardViz, Slot slot = null)
        {
            if (cardViz != null)
            {
                var r = actLogic.UnholdCard(cardViz, slot);
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
                slotViz.CloseSlot();
            }
        }

        private void OpenSlot(Slot slot, List<SlotViz> slots)
        {
            foreach (var slotViz in slots)
            {
                if (slotViz.gameObject.activeSelf == false)
                {
                    slotViz.LoadSlot(slot);
                    slotViz.OpenSlot();
                    break;
                }
            }
        }

        public void SetupResultCards(List<CardViz> cards)
        {
            resultLane.PlaceCards(cards);
            tokenViz.SetResultCount(cards.Count);

            ApplyStatus(ActStatus.Finished);
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
                ApplyStatus(ActStatus.Ready);
            }
            else
            {
                ApplyStatus(ActStatus.Idle);
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
                aspectBar.Load(actLogic.fragments);
                cardBar.Load(actLogic.fragments);
            }
        }

        public void ApplyStatus(ActStatus actStatus)
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
                        label.text = tokenViz?.token?.label;
                        text.text = tokenViz?.token?.description;
                    }
                    break;
                case ActStatus.Ready:
                    if (readyAct != null)
                    {
                        okButton.interactable = true;
                        label.text = readyAct.label;
                        text.text = readyAct.text;
                    }
                    break;
                case ActStatus.Running:
                    timer.gameObject.SetActive(true);
                    idleSlotsGO.SetActive(false);
                    runSlotsGO.SetActive(true);
                    okButton.interactable = false;
                    label.text = runLabel;
                    text.text = runText;
                    break;
                case ActStatus.Finished:
                    timer.gameObject.SetActive(false);
                    runSlotsGO.SetActive(false);
                    resultLane.gameObject.SetActive(true);
                    collectButton.interactable = true;
                    text.text = actLogic.activeAct.endText;
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

            timer.gameObject.SetActive(false);
            collectButton.interactable = false;
            tokenViz.SetResultCount(0);
            gameObject.SetActive(false);

            CloseSlots(idleSlots);
            CloseSlots(runSlots);

            foreach(var c in gameObject.GetComponentsInChildren<Canvas>())
            {
                c.worldCamera = Camera.main;
            }

            if (tokenViz.autoPlay != null)
            {
                readyAct = tokenViz.autoPlay;
                GoForIt();
            }
            else
            {
                StatusIdle();
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
