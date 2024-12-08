using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;


namespace CultistLike
{
    public class ActWindow : Drag
    {
        [HideInInspector] public bool open;

        [Header("Layout")]
        [SerializeField] private TextMeshPro label;
        [SerializeField] private TextMeshProUGUI text;
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

        public ActStatus actStatus;
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
            open = true;
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
            open = false;
            GameManager.Instance.CloseWindow();
        }

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

        public void ParentCardToWindow(CardViz cardViz)
        {
            cardViz.Hide();
            cardViz.free = false;
            cardViz.transform.SetParent(transform);
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
                if (slotViz.slottedCard == null || slotViz.cardLock == false)
                {
                    slotViz.SetHighlight(p);
                }
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

        // closing a slot will remove a slotted card
        // since slotted cards influence which slots are open
        // reUpdate needs to be done
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
                        UpdateBars();
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

        public void UpdateBars()
        {
            if (suspendUpdates == false)
            {
                aspectBar?.Load(actLogic.fragments);
                cardBar?.Load(actLogic.fragments);
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
                        tokenViz.SetResultCount(0);
                    }
                    break;
                case ActStatus.Running:
                    timer.gameObject.SetActive(true);
                    idleSlotsGO.SetActive(false);
                    runSlotsGO.SetActive(true);
                    okButton.interactable = false;
                    tokenViz.SetResultCount(0);
                    label.text = runLabel;
                    text.text = runText;
                    break;
                case ActStatus.Finished:
                    timer.gameObject.SetActive(false);
                    runSlotsGO.SetActive(false);
                    resultLane.gameObject.SetActive(true);
                    collectButton.interactable = true;
                    label.text = runLabel;
                    text.text = actLogic.activeAct.endText;
                    break;
                default:
                    break;
            }
        }

        public ActWindowSave Save()
        {
            var save = new ActWindowSave();
            save.open = open;
            save.actStatus = actStatus;
            save.readyAct = readyAct;
            save.position = transform.position;

            save.slots = new List<SlotVizSave>();
            foreach (var slotViz in slots)
            {
                if (slotViz.gameObject.activeSelf == true)
                {
                    save.slots.Add(slotViz.Save());
                }
            }

            return save;
        }

        public void Load(ActWindowSave save, TokenViz tokenViz)
        {
            transform.position = save.position;
            actStatus = save.actStatus;
            readyAct = save.readyAct;
            open = save.open;
            LoadToken(tokenViz);

            for (int i=0; i<save.slots.Count && i<slots.Count; i++)
            {
                slots[i].Load(save.slots[i]);
            }

            if (save.open == true)
            {
                BringUp();
            }
        }

        private void AttemptReadyAct()
        {
            if (tokenViz.token != null)
            {
                foreach (var act in GameManager.Instance.initialActs)
                {
                    if (act.token == tokenViz.token)
                    {
                        readyAct = actLogic.AttemptAct(act);
                        if (readyAct != null)
                        {
                            break;
                        }
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

        private IEnumerable<SlotViz> MatchingSlots(CardViz cardViz, bool onlyEmpty = false)
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

        private void Awake()
        {
            actLogic = GetComponent<ActLogic>();
        }

        private void Start()
        {
            GetComponent<Drag>().draggingPlane = GameManager.Instance.windowPlane;

            timer.gameObject.SetActive(false);
            okButton.interactable = false;
            collectButton.interactable = false;

            gameObject.SetActive(open);

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
                UpdateSlots();
                ApplyStatus(actStatus);
            }
        }
    }

    [Serializable]
    public class ActWindowSave
    {
        public bool open;
        public ActStatus actStatus;
        public Act readyAct;
        public List<SlotVizSave> slots;
        public Vector3 position;
    }

    public enum ActStatus
    {
            Idle,
            Ready,
            Running,
            Finished
    }
}
